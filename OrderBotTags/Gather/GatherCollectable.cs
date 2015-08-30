namespace ExBuddy.OrderBotTags.Gather
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using Buddy.Coroutines;

    using Clio.Utilities;
    using Clio.XmlEngine;

    using ExBuddy.OrderBotTags.Gather.Rotations;

    using ff14bot;
    using ff14bot.Enums;
    using ff14bot.Helpers;
    using ff14bot.Managers;
    using ff14bot.NeoProfiles;
    using ff14bot.Objects;

    using TreeSharp;

    using Action = TreeSharp.Action;

    [XmlElement("GatherCollectable")]
    public class GatherCollectable : ProfileBehavior
    {
        private static readonly Dictionary<string, Type> Rotations;

        private readonly SpellData cordialSpellData = DataManager.GetItem((uint)CordialType.Cordial).BackingAction;

        private bool isDone;

        private Func<bool> freeRangeConditionFunc;

        private IGatheringRotation gatherRotation;

        private ushort gatherRotationGp;

        private byte gatherRotationTime;

        internal IGatherSpot GatherSpot;

        internal GatheringPointObject Node;

        static GatherCollectable()
        {
            Rotations = LoadRotationTypes();
        }

        public override bool IsDone
        {
            get
            {
                return isDone;
            }
        }

        public int AdjustedWaitForGp
        {
            get
            {
                // Return the lower of your MaxGP rounded down to the nearest 50.
                return Math.Min(Core.Player.MaxGP - (Core.Player.MaxGP % 50), gatherRotationGp);
            }
        }

        internal GatheringItem GatherItem { get; private set; }

        [DefaultValue(true)]
        [XmlAttribute("AlwaysGather")]
        public bool AlwaysGather { get; set; }

        [DefaultValue(CordialTime.BeforeGather)]
        [XmlElement("CordialTime")]
        public CordialTime CordialTime { get; set; }

        [DefaultValue(CordialType.Auto)]
        [XmlElement("CordialType")]
        public CordialType CordialType { get; set; }

        [XmlAttribute("FreeRange")]
        public bool FreeRange { get; set; }

        [DefaultValue("Condition.TrueFor(1, TimeSpan.FromHours(1))")]
        [XmlAttribute("FreeRangeCondition")]
        public string FreeRangeCondition { get; set; }

        [DefaultValue(45)]
        [XmlAttribute("MountId")]
        public int MountId { get; set; }

        [DefaultValue(true)]
        [XmlAttribute("LogFlight")]
        public bool LogFlight { get; set; }

        [DefaultValue(5)]
        [XmlAttribute("Slot")]
        public int Slot { get; set; }

        [XmlElement("GatherObjects")]
        public List<string> GatherObjects { get; set; }

        [DefaultValue("Collect470")]
        [XmlElement("GatherRotation")]
        public string GatherRotation { get; set; }

        [XmlElement("GatherSpots")]
        public List<StealthApproachGatherSpot> GatherSpots { get; set; }

        [DefaultValue(GatherStrategy.CollectOnce)]
        [XmlElement("GatherStrategy")]
        public GatherStrategy GatherStrategy { get; set; }

        [XmlElement("ItemNames")]
        public List<string> ItemNames { get; set; }

        [DefaultValue(3.0f)]
        [XmlAttribute("Distance")]
        public float Distance { get; set; }

        [DefaultValue(2.0f)]
        [XmlAttribute("Radius")]
        public float Radius { get; set; }

        [DefaultValue(5.0f)]
        [XmlAttribute("NavHeight")]
        public float NavHeight { get; set; }

        protected override void OnResetCachedDone()
        {
            if (!isDone)
            {
                Logging.Write(Colors.Chartreuse, "GatherCollectable: Resetting.");
            }

            isDone = false;
            GatherSpot = null;
            gatherRotation = null;
            gatherRotationGp = 0;
            gatherRotationTime = 0;
            Node = null;
            GatherItem = null;
            freeRangeConditionFunc = null;
        }

        protected override void OnDone()
        {
            GatherItem = null;
        }

        protected override Composite CreateBehavior()
        {
            // Had to add null check for node.
            return
                new PrioritySelector(
                    new Decorator(
                        ret => Node != null && (!Node.IsValid || (FreeRange && Node.Location.Distance3D(Core.Player.Location) > Radius)),
                        new Action(r => OnResetCachedDone())),
                    new Decorator(
                        ret => Node == null,
                        new Sequence(
                            new ActionRunCoroutine(ctx => FindNode()),
                            new Action(r => MovementManager.SetFacing2D(Node.Location)))),
                    new Decorator(
                        ret => Node != null && Node.IsValid && GatherSpot == null,
                        new ActionRunCoroutine(ctx => FindGatherSpot())),
                    new Decorator(
                        ret => Node != null && Node.IsValid && GatherSpot != null && gatherRotation == null,
                        new ActionRunCoroutine(ctx => ResolveGatherRotation())),
                    new Decorator(
                        ret =>
                        Node != null && Node.IsValid && GatherSpot != null && !FreeRange && Node.Location.Distance3D(Core.Player.Location) > Distance,
                        new ActionRunCoroutine(ctx => MoveToGatherSpot())),
                    new Decorator(
                        ret =>
                        Node != null && Node.IsValid && GatherSpot != null && Node.CanGather
                        && Node.Location.Distance3D(Core.Player.Location) <= Distance,
                        new Sequence(
                            new ActionRunCoroutine(ctx => BeforeGather()),
                            new ActionRunCoroutine(ctx => Gather()),
                            new ActionRunCoroutine(ctx => AfterGather()))),
                    new Decorator(
                        ret => Node != null && Node.IsValid && GatherSpot != null && !FreeRange && !Node.CanGather,
                        new ActionRunCoroutine(ctx => MoveFromGatherSpot())));
        }

        private static Dictionary<string, Type> LoadRotationTypes()
        {
            Type[] types = null;
            try
            {
                types =
                    Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(t => !t.IsAbstract && typeof(IGatheringRotation).IsAssignableFrom(t))
                        .ToArray();
            }
            catch
            {
                Logging.Write("Unable to get types, Loading Known Rotations.");
            }

            if (types == null)
            {
                types = GetKnownRotationTypes();
            }

            ReflectionHelper.CustomAttributes<GatheringRotationAttribute>.RegisterTypes(types);

            foreach (var type in types)
            {
                Logging.Write(
                    Colors.Chartreuse,
                    "GatherCollectable: Loaded Rotation -> {0}, GP: {1}, Time: {2}",
                    type.GetCustomAttributePropertyValue<GatheringRotationAttribute, string>(
                        attr => attr.Name,
                        type.Name.Replace("GatheringRotation", string.Empty)),
                        type.GetCustomAttributePropertyValue<GatheringRotationAttribute, ushort>(
                            attr => attr.RequiredGp),
                        type.GetCustomAttributePropertyValue<GatheringRotationAttribute, byte>(
                            attr => attr.RequiredTimeInSeconds));
            }

            var dict =
                types.ToDictionary(
                    k => k.GetCustomAttributePropertyValue<GatheringRotationAttribute, string>(attr => attr.Name, k.Name.Replace("GatheringRotation", string.Empty)), v => v);

            return dict;

        }

        private static Type[] GetKnownRotationTypes()
        {
            return new[]
                       {
                            typeof(UnspoiledGatheringRotation) ,
                            typeof(DefaultCollectGatheringRotation),
                            typeof(Collect470GatheringRotation),
                            typeof(Collect450GatheringRotation),
                            typeof(Collect550GatheringRotation),
                            typeof(Collect570GatheringRotation),
                            typeof(MapGatheringRotation),
                            typeof(OverrideUnspoiledGatheringRotation)
                       };
        }

        private async Task<bool> ResolveGatherRotation()
        {
            Type rotationType;
            if (!Rotations.TryGetValue(GatherRotation, out rotationType))
            {
                rotationType = typeof(DefaultCollectGatheringRotation);
                Logging.Write(Colors.PaleVioletRed, "GatherCollectable: Could not find rotation, using DefaultCollect instead.");
            }

            gatherRotation = rotationType.CreateInstance<IGatheringRotation>();
            gatherRotationGp =
                rotationType.GetCustomAttributePropertyValue<GatheringRotationAttribute, ushort>(p => p.RequiredGp);
            gatherRotationTime =
                rotationType.GetCustomAttributePropertyValue<GatheringRotationAttribute, byte>(
                    p => p.RequiredTimeInSeconds);

            Logging.Write(Colors.Chartreuse, "GatherCollectable: Using rotation -> " + rotationType.GetCustomAttributePropertyValue<GatheringRotationAttribute, string>(p => p.Name, rotationType.Name.Replace("GatheringRotation", string.Empty)));

            return true;
        }

        private async Task<bool> FindGatherSpot()
        {
            if (GatherSpots != null && Node.Location.Distance3D(Core.Player.Location) > Distance)
            {
                GatherSpot = GatherSpots.FirstOrDefault(gs => gs != null && gs.IsMatch);
            }

            // Either GatherSpots is null, the node is already in range, or there are no matches, use fallback
            if (GatherSpot == null)
            {
                GatherSpot = new GatherSpot { NodeLocation = Node.Location, UseMesh = true };
            }

            Logging.Write(Colors.Chartreuse, "GatherCollectable: GatherSpot set -> " + GatherSpot);

            return true;
        }

        private async Task<bool> FindNode()
        {
            IEnumerable<GatheringPointObject> nodes;

            if (FreeRange)
            {
                nodes =
                    GameObjectManager.GetObjectsOfType<GatheringPointObject>()
                        .Where(gpo => gpo.Distance2D(Core.Player.Location) < Radius);
            }
            else
            {
                nodes = GameObjectManager.GetObjectsOfType<GatheringPointObject>();
            }

            if (GatherObjects != null)
            {
                Node =
                    nodes
                        .OrderBy(gpo => GatherObjects.FindIndex(i => string.Equals(gpo.EnglishName, i, StringComparison.InvariantCultureIgnoreCase)))
                        .FirstOrDefault(
                            gpo => GatherObjects.Contains(gpo.EnglishName, StringComparer.InvariantCultureIgnoreCase)
                            && gpo.CanGather);
            }
            else
            {
                Node = nodes.FirstOrDefault(gpo => gpo.CanGather);
            }

            if (Node == null)
            {
                if (FreeRange && !FreeRangeConditional())
                {
                    await Coroutine.Sleep(100);
                    isDone = true;
                }

                return false;
            }

            var entry = Blacklist.GetEntry(Node.ObjectId);
            if (entry != null && entry.Flags.HasFlag(BlacklistFlags.Interact))
            {
                Logging.Write(
                    Colors.PaleVioletRed,
                    "Node on blacklist, waiting until we move out of range or it clears.");

                if (await Coroutine.Wait(entry.Length, () => Node.Location.Distance3D(Core.Player.Location) > Radius))
                {
                    Node = null;
                    Logging.Write(Colors.Chartreuse, "GatherCollectable: Node cleared");
                    return false;
                }
            }

            Logging.Write(Colors.Chartreuse, "GatherCollectable: Node set -> " + Node);

            return true;
        }

        private async Task<bool> MoveToGatherSpot()
        {
            var result =
                await
                GatherSpot.MoveToSpot(
                    () => Actions.CastAura(Ability.Stealth, AbilityAura.Stealth),
                    Node.Location,
                    (uint)MountId,
                    Radius,
                    NavHeight,
                    "Gather Spot",
                    LogFlight);
            return result;
        }

        private async Task<bool> MoveFromGatherSpot()
        {
            var result = await GatherSpot.MoveFromSpot();

            isDone = true;
            return result;
        }

        private async Task<bool> BeforeGather()
        {
            //TODO: Fix the logic so it is easier to read
            if (Core.Player.CurrentGP >= AdjustedWaitForGp)
            {
                return true;
            }

            var eorzeaMinutesTillDespawn = 55 - WorldManager.EorzaTime.Minute;
            var realSecondsTillDespawn = eorzeaMinutesTillDespawn * 35 / 12;
            var realSecondsTillStartGathering = realSecondsTillDespawn - gatherRotationTime;

            if (realSecondsTillStartGathering < 1)
            {
                if (gatherRotation.ForceGatherIfMissingGpOrTime)
                {
                    return true;
                }

                Logging.Write("Not enough time to gather");
                isDone = true;
                return true;
            }

            var ticksTillStartGathering = realSecondsTillStartGathering / 3;

            var gp = Core.Player.CurrentGP + ticksTillStartGathering * 5;

            if (CordialType <= CordialType.None)
            {
                Logging.Write("Cordial not enabled.  To enable cordial use, add the 'cordialType' attribute with value 'Auto', 'Cordial', or 'HiCordial'");

                if (gatherRotation.ForceGatherIfMissingGpOrTime)
                {
                    return true;
                }

                if (gp >= AdjustedWaitForGp)
                {
                    return await WaitForGpRegain();
                }

                Logging.Write("Not enough time to gather");
                isDone = true;
                return true;
            }

            if (gatherRotation.ForceGatherIfMissingGpOrTime)
            {
                return true;
            }

            if (gp >= AdjustedWaitForGp || !CordialTime.HasFlag(CordialTime.BeforeGather))
            {
                return await WaitForGpRegain();
            }
            if (realSecondsTillStartGathering < cordialSpellData.Cooldown.TotalSeconds)
            {
                return true;
            }

            if (gp + 300 >= AdjustedWaitForGp)
            {
                // If we used the cordial or the CordialType is only Cordial, not Auto or HiCordial, then return
                if (await UseCordial(CordialType.Cordial, realSecondsTillStartGathering) || CordialType == CordialType.Cordial)
                {
                    if (gatherRotation.ForceGatherIfMissingGpOrTime)
                    {
                        return true;
                    }

                    return await WaitForGpRegain();
                }
            }

            // Recalculate: could have no time left at this point
            eorzeaMinutesTillDespawn = 55 - WorldManager.EorzaTime.Minute;
            realSecondsTillDespawn = eorzeaMinutesTillDespawn * 35 / 12;
            realSecondsTillStartGathering = realSecondsTillDespawn - gatherRotationTime;

            if (realSecondsTillStartGathering < 1)
            {
                if (gatherRotation.ForceGatherIfMissingGpOrTime)
                {
                    return true;
                }

                Logging.Write("Not enough GP to gather");
                isDone = true;
                return true;
            }

            ticksTillStartGathering = realSecondsTillStartGathering / 3;

            gp = Core.Player.CurrentGP + ticksTillStartGathering * 5;

            if (gp + 400 >= AdjustedWaitForGp)
            {
                if (await UseCordial(CordialType.HiCordial, realSecondsTillStartGathering))
                {
                    if (gatherRotation.ForceGatherIfMissingGpOrTime)
                    {
                        return true;
                    }

                    return await WaitForGpRegain();
                }
            }

            if (gatherRotation.ForceGatherIfMissingGpOrTime)
            {
                return true;
            }

            return await WaitForGpRegain();
        }

        private async Task<bool> WaitForGpRegain()
        {
            var eorzeaMinutesTillDespawn = 55 - WorldManager.EorzaTime.Minute;
            var realSecondsTillDespawn = eorzeaMinutesTillDespawn * 35 / 12;
            var realSecondsTillStartGathering = realSecondsTillDespawn - gatherRotationTime;

            if (realSecondsTillStartGathering < 1)
            {
                if (gatherRotation.ForceGatherIfMissingGpOrTime)
                {
                    return true;
                }

                Logging.Write("Not enough GP to gather");
                isDone = true;
                return true;
            }

            if (Core.Player.CurrentGP < AdjustedWaitForGp)
            {
                Logging.Write(
                    "Waiting for GP, Seconds Until Gathering: " + realSecondsTillStartGathering + ", Current GP: "
                    + Core.Player.CurrentGP + ", WaitForGP: " + AdjustedWaitForGp);
                await
                Coroutine.Wait(
                    TimeSpan.FromSeconds(realSecondsTillStartGathering),
                    () => Core.Player.CurrentGP >= AdjustedWaitForGp);
            }

            return true;
        }

        private async Task<bool> AfterGather()
        {
            if (Core.Player.CurrentGP < AdjustedWaitForGp && CordialTime.HasFlag(CordialTime.BeforeGather))
            {

            }

            if (FreeRange)
            {
                //TODO: check this.
                // We want to reset here if Free Range, but we need to implement looping and possibly hotspots
                // OnResetCachedDone();
                isDone = true;
            }

            return true;
        }

        private async Task<bool> UseCordial(CordialType cordialType, int maxTimeoutSeconds = 5)
        {
            if (cordialSpellData.Cooldown.TotalSeconds < maxTimeoutSeconds)
            {
                var cordial =
                    InventoryManager.FilledSlots.FirstOrDefault(
                        slot => slot.Item.Id == (uint)cordialType);

                if (cordial != null)
                {
                    Logging.Write("Using Cordial -> Waiting (sec): " + maxTimeoutSeconds + " CurrentGP: " + Core.Player.CurrentGP);
                    if (await Coroutine.Wait(TimeSpan.FromSeconds(maxTimeoutSeconds),
                        () =>
                        {
                            if (Core.Player.IsMounted)
                            {
                                Logging.Write("Dismounting to use cordial.");
                                Actionmanager.Dismount();
                                return false;
                            }

                            return cordial.CanUse(Core.Player);
                        }))
                    {
                        cordial.UseItem(Core.Player);
                        Logging.Write("Using Cordial: " + cordialType);
                        return true;
                    }
                }
            }

            return false;
        }

        private async Task<bool> Gather()
        {
            await
                Actions.CastAura(
                    Ability.Truth,
                    Core.Player.CurrentJob == ClassJobType.Miner ? AbilityAura.TruthOfMountains : AbilityAura.TruthOfForests);

            if (Poi.Current.Unit != Node)
            {
                Poi.Current = new Poi(Node, PoiType.Gather);
            }

            if (!Blacklist.Contains(Poi.Current.Unit, BlacklistFlags.Interact))
            {
                Blacklist.Add(Poi.Current.Unit, BlacklistFlags.Interact, TimeSpan.FromSeconds(Math.Max(gatherRotationTime + 15, 30)), "Blacklisting node so that we don't retarget -> " + Poi.Current.Unit);
            }

            var attempts = 0;
            while (attempts < 3 && !GatheringManager.WindowOpen)
            {
                Poi.Current.Unit.Interact();

                if (!await Coroutine.Wait(3000, () => GatheringManager.WindowOpen))
                {
                    if (FreeRange)
                    {
                        Logging.Write("Gathering Window didn't open: Retrying in 3 seconds. " + ++attempts);
                        continue;
                    }

                    Logging.Write("Gathering Window didn't open: Re-attempting to move into place. " + ++attempts);
                    GatherSpot = new GatherSpot { NodeLocation = Node.Location, UseMesh = true };

                    await MoveToGatherSpot();
                }
            }

            if (!GatheringManager.WindowOpen)
            {
                OnResetCachedDone();
                return true;
            }

            await Coroutine.Sleep(2200);

            if (!ResolveGatherItem())
            {
                isDone = true;
                return false;
            }

            CheckForGatherRotationOverride();

            await gatherRotation.Prepare(this);
            await gatherRotation.ExecuteRotation(this);
            await gatherRotation.Gather(this);

            Poi.Clear("Gather Complete!");

            await Coroutine.Wait(6000, () => !Node.CanGather);

            return true;
        }

        internal bool ResolveGatherItem()
        {
            GatherItem = null;
            var windowItems = GatheringManager.GatheringWindowItems;
            if (ItemNames == null || ItemNames.Count == 0)
            {
                GatherItem = GatheringManager.GetGatheringItemByIndex((uint)Slot);
            }

            if (GatherItem == null && ItemNames != null)
            {
                foreach (var itemName in ItemNames)
                {
                    GatherItem =
                        windowItems.FirstOrDefault(
                            i =>
                            i.IsFilled && !i.IsUnknown
                            && string.Equals(
                                itemName,
                                i.ItemData.EnglishName,
                                StringComparison.InvariantCultureIgnoreCase));

                    if (GatherItem != null && (!GatherItem.ItemData.Unique || GatherItem.ItemData.ItemCount() == 0))
                    {
                        return true;
                    }
                }
            }

            if (GatherItem == null && !AlwaysGather)
            {
                Poi.Clear("Skipping this node, no items we want to gather.");
                var window = RaptureAtkUnitManager.GetWindowByName("Gathering");
                window.SendAction(1, 3, 0xFFFFFFFF);

                return false;
            }

            if (GatherItem != null)
            {
                return true;
            }

            GatherItem =
                windowItems.OrderByDescending(i => i.SlotIndex)
                    .FirstOrDefault(i => i.IsFilled && !i.IsUnknown && i.ItemId < 20) // Try to gather cluster/crystal/shard
                ?? windowItems.FirstOrDefault(i => !i.ItemData.Unique && !i.ItemData.Untradeable && i.ItemData.ItemCount() > 0) // Try to collect
                ?? windowItems.Where(i => !i.ItemData.Unique && !i.ItemData.Untradeable).OrderByDescending(i => i.SlotIndex).First(); // Take last item that is not unique or untradeable

            Logging.Write(Colors.Chartreuse, "GatherCollectable: could not find item by slot or name, gathering" + GatherItem.ItemData);

            return true;
        }

        private void CheckForGatherRotationOverride()
        {
            if (!gatherRotation.CanOverride)
            {
                return;
            }

            var rotationType = gatherRotation.GetType();

            var rotationAndType = Rotations.Where(kvp => kvp.Value.GUID != rotationType.GUID)
                .Select(r => new { Rotation= r.Value.CreateInstance<IGatheringRotation>(), Type = r.Value})
                .OrderBy(r => r.Rotation.ShouldOverrideSelectedGatheringRotation(this))
                .FirstOrDefault();

            if (rotationAndType == null)
            {
                return;
            }
            
            Logging.Write(
                Colors.Chartreuse,
                "GatherCollectable: Rotation Override -> Old: "
                + rotationType.GetCustomAttributePropertyValue<GatheringRotationAttribute, string>(
                attr => attr.Name,
                    rotationType.Name.Replace("GatheringRotation", string.Empty))
                + " , New: "
                + rotationAndType.Type.GetCustomAttributePropertyValue<GatheringRotationAttribute, string>(
                    attr => attr.Name,
                    rotationAndType.Type.Name.Replace("GatheringRotation", string.Empty)));

            gatherRotation = rotationAndType.Rotation;
        }

        private bool FreeRangeConditional()
        {
            if (freeRangeConditionFunc == null)
            {
                freeRangeConditionFunc = ScriptManager.GetCondition(FreeRangeCondition);
            }

            return freeRangeConditionFunc();
        }
    }
}
