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
    using ff14bot.Behavior;
    using ff14bot.Enums;
    using ff14bot.Helpers;
    using ff14bot.Managers;
    using ff14bot.Navigation;
    using ff14bot.NeoProfiles;
    using ff14bot.Objects;

    using TreeSharp;

    using Action = TreeSharp.Action;

    [XmlElement("GatherCollectable")]
    public class GatherCollectableTag : ProfileBehavior
    {
        private static readonly Dictionary<string, IGatheringRotation> Rotations;

        private static readonly SpellData CordialSpellData = DataManager.GetItem((uint)CordialType.Cordial).BackingAction;

        private bool isDone;

        private int loopCount;

        private Func<bool> freeRangeConditionFunc;

        private IGatheringRotation initialGatherRotation;

        private IGatheringRotation gatherRotation;

        internal IGatherSpot GatherSpot;

        internal GatheringPointObject Node;

        internal GatheringItem GatherItem;

        internal Collectable CollectableItem;

        private Func<bool> whileFunc;

        private DateTime startTime;

        static GatherCollectableTag()
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
                var requiredGp = this.gatherRotation == null ? 0 : this.gatherRotation.Attributes.RequiredGp;

                // Return the lower of your MaxGP rounded down to the nearest 50.
                return Math.Min(Core.Player.MaxGP - (Core.Player.MaxGP % 50), requiredGp);
            }
        }

        [DefaultValue(true)]
        [XmlAttribute("AlwaysGather")]
        public bool AlwaysGather { get; set; }

        [DefaultValue(CordialTime.BeforeGather)]
        [XmlAttribute("CordialTime")]
        public CordialTime CordialTime { get; set; }

        [DefaultValue(CordialType.None)]
        [XmlAttribute("CordialType")]
        public CordialType CordialType { get; set; }

        [XmlAttribute("DiscoverUnknowns")]
        public bool DiscoverUnknowns { get; set; }

        [XmlAttribute("FreeRange")]
        public bool FreeRange { get; set; }

        [DefaultValue("Condition.TrueFor(1, TimeSpan.FromHours(1))")]
        [XmlAttribute("FreeRangeCondition")]
        public string FreeRangeCondition { get; set; }

        [DefaultValue(45)]
        [XmlAttribute("MountId")]
        public int MountId { get; set; }

        [XmlAttribute("LogFlight")]
        public bool LogFlight { get; set; }

        // I want this to be an attribute, but for backwards compatibilty, we will use element
        [DefaultValue(-1)]
        [XmlElement("Slot")]
        public int Slot { get; set; }

        // Backwards compatibility
        [XmlElement("GatherObject")]
        public string GatherObject { get; set; }

        [XmlElement("GatherObjects")]
        public List<string> GatherObjects { get; set; }

        // Maybe this should be an attribute?
        [DefaultValue("RegularNode")]
        [XmlElement("GatherRotation")]
        public string GatherRotation { get; set; }

        [XmlElement("GatherSpots")]
        public List<StealthApproachGatherSpot> GatherSpots { get; set; }

        [DefaultValue(GatherStrategy.GatherOrCollect)]
        [XmlAttribute("GatherStrategy")]
        public GatherStrategy GatherStrategy { get; set; }

        [XmlElement("HotSpots")]
        public IndexedList<HotSpot> HotSpots { get; set; }

        [XmlElement("Collectables")]
        public List<Collectable> Collectables { get; set; }

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

        [DefaultValue(250)]
        [XmlAttribute("SpellDelay")]
        public int SpellDelay { get; set; }

        [DefaultValue(2000)]
        [XmlAttribute("WindowDelay")]
        public int WindowDelay { get; set; }

        [DefaultValue(-1)]
        [XmlAttribute("Loops")]
        public int Loops { get; set; }

        [XmlAttribute("SpawnTimeout")]
        public int SpawnTimeout { get; set; }

        [DefaultValue("True")]
        [XmlAttribute("While")]
        public string While { get; set; }

        // TODO: Look into making this use Type instead of Enum
        [DefaultValue(GatherSpotType.GatherSpot)]
        [XmlAttribute("DefaultGatherSpotType")]
        public GatherSpotType DefaultGatherSpotType { get; set; }

        protected bool Condition()
        {
            if (whileFunc == null)
            {
                whileFunc = ScriptManager.GetCondition(While);
            }

            return whileFunc();
        }

        protected override void OnResetCachedDone()
        {
            if (!isDone)
            {
                Logging.Write(Colors.Chartreuse, "GatherCollectable: Resetting.");
            }

            isDone = false;
            loopCount = 0;
            ResetInternal();
        }

        internal void ResetInternal()
        {
            GatherSpot = null;
            Node = null;
            GatherItem = null;
            CollectableItem = null;
        }

        protected override void OnStart()
        {
            // Ensure positive values
            WindowDelay = WindowDelay > 0 ? WindowDelay : 2000;
            SpellDelay = SpellDelay > 0 ? SpellDelay : 250;

            if (HotSpots != null)
            {
                HotSpots.IsCyclic = Loops < 1;
            }

            // backwards compatibility
            if (GatherObjects == null && !string.IsNullOrWhiteSpace(GatherObject))
            {
                GatherObjects = new List<string>{ GatherObject };
            }

            startTime = DateTime.Now;
        }

        protected override Composite CreateBehavior()
        {
            return
                new PrioritySelector(
                    new Decorator(
                        ret => !Condition(),
                        new Action(r => isDone = true)),
                        new Decorator(
                            ret => !MovementManager.IsFlying && Core.Player.ClassLevel >= 46 && !Core.Player.HasAura((int)(Core.Player.CurrentJob == ClassJobType.Miner ? AbilityAura.TruthOfMountains : AbilityAura.TruthOfForests)),
                            new ActionRunCoroutine(ctx => CastTruth())),
                    new Decorator(
                        ret => Node != null && (!Node.IsValid || (FreeRange && Node.Location.Distance2D(Core.Player.Location) > Radius)),
                        new Action(r => OnResetCachedDone())),
                    new Decorator(
                        ret => HotSpots != null && !HotSpots.CurrentOrDefault.WithinHotSpot2D(Core.Player.Location),
                        new ActionRunCoroutine(ctx => Behaviors.MoveTo(HotSpots.CurrentOrDefault, true, (uint)MountId, HotSpots.CurrentOrDefault.Radius -1, NavHeight, HotSpots.CurrentOrDefault.Name, LogFlight))),
                    new Decorator(
                        ret => (HotSpots == null || HotSpots.CurrentOrDefault.WithinHotSpot2D(Core.Player.Location)) && Node == null,
                        new Sequence(
                            new ActionRunCoroutine(ctx => FindNode()),
                            new Decorator(
                                ret => HotSpots == null,
                            new Action(r => MovementManager.SetFacing2D(Node.Location))))),
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
                        new Sequence(
                        new ActionRunCoroutine(ctx => MoveFromGatherSpot()),
                        new ActionRunCoroutine(ctx => ResetOrDone()))));
        }

        private static Dictionary<string, IGatheringRotation> LoadRotationTypes()
        {
            Type[] types = null;
            try
            {
                types =
                    Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(t => !t.IsAbstract && typeof(IGatheringRotation).IsAssignableFrom(t) && t.GetCustomAttribute<GatheringRotationAttribute>() != null)
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

            var instances = types.Select(t => t.CreateInstance<IGatheringRotation>()).ToArray();

            foreach (var instance in instances)
            {
                Logging.Write(
                    Colors.Chartreuse,
                    "GatherCollectable: Loaded Rotation -> {0}, GP: {1}, Time: {2}",
                    instance.Attributes.Name,
                    instance.Attributes.RequiredGp,
                    instance.Attributes.RequiredTimeInSeconds);
            }


            var dict =
                instances.ToDictionary(
                    k => k.Attributes.Name, v => v);

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
            IGatheringRotation rotation;
            if (!Rotations.TryGetValue(GatherRotation, out rotation))
            {
                rotation = new RegularNodeGatheringRotation();
                Logging.Write(Colors.PaleVioletRed, "GatherCollectable: Could not find rotation, using RegularNode instead.");
            }

            initialGatherRotation = gatherRotation = rotation;

            Logging.Write(Colors.Chartreuse, "GatherCollectable: Using rotation -> " + rotation.Attributes.Name);

            return true;
        }

        private async Task<bool> CastTruth()
        {
            while (Core.Player.IsMounted)
            {
                await CommonTasks.StopAndDismount();
                await Coroutine.Yield();
            }

            await
                CastAura(
                    Ability.Truth,
                    Core.Player.CurrentJob == ClassJobType.Miner ? AbilityAura.TruthOfMountains : AbilityAura.TruthOfForests);

            return true;
        }
        
        private async Task<bool> ResetOrDone()
        {
            if (HotSpots == null || HotSpots.Count == 0)
            {
                isDone = true;
            }
            else
            {
                ResetInternal();
                await ChangeHotSpot();
            }

            return true;
        }

        private async Task<bool> ChangeHotSpot()
        {
            if (SpawnTimeout > 0 && DateTime.Now < startTime.AddSeconds(SpawnTimeout))
            {
                return false;
            }

            if (HotSpots != null)
            {
                // If finished current loop and set to not cyclic (we know this because if it was cyclic Next is always true)
                if (!HotSpots.Next())
                {
                    // If finished all loops, otherwise just incrementing loop count
                    if (++loopCount == Loops)
                    {
                        isDone = true;
                        return true;
                    }

                    // If not cyclic and it is on the last index
                    if (!HotSpots.IsCyclic && HotSpots.Index == HotSpots.Count - 1)
                    {
                        HotSpots.Index = 0;
                    }
                }
            }

            return true;
        }

        private async Task<bool> FindGatherSpot()
        {
            if (GatherSpots != null && Node.Location.Distance2D(Core.Player.Location) > Distance)
            {
                GatherSpot = GatherSpots.OrderBy(gs => gs.NodeLocation.Distance2D(Node.Location)).FirstOrDefault();
            }

            // Either GatherSpots is null, the node is already in range, or there are no matches, use fallback
            if (GatherSpot == null)
            {
                SetFallbackGatherSpot(Node.Location, true);
            }

            Logging.Write(Colors.Chartreuse, "GatherCollectable: GatherSpot set -> " + GatherSpot);

            return true;
        }

        private async Task<bool> FindNode(bool retryCenterHotspot = true)
        {
            while (true)
            {
                IEnumerable<GatheringPointObject> nodes = GameObjectManager.GetObjectsOfType<GatheringPointObject>().Where(gpo => gpo.CanGather).ToArray();

                if (GatherStrategy == GatherStrategy.TouchAndGo && HotSpots != null)
                {
                    if (GatherObjects != null)
                    {
                        nodes = nodes.Where(gpo => GatherObjects.Contains(gpo.EnglishName, StringComparer.InvariantCultureIgnoreCase));
                    }

                    foreach (var node in nodes.OrderBy(gpo => gpo.Location.Distance2D(Core.Player.Location)).Where(gpo => HotSpots.CurrentOrDefault.WithinHotSpot2D(gpo.Location)).Skip(1))
                    {
                        if (!Blacklist.Contains(node.ObjectId, BlacklistFlags.Interact))
                        {
                            Blacklist.Add(node, BlacklistFlags.Interact, TimeSpan.FromSeconds(30), "Skip furthest nodes in hotspot. We only want 1.");
                        }
                    }
                }

                nodes = nodes.Where(gpo => !Blacklist.Contains(gpo.ObjectId, BlacklistFlags.Interact));

                if (FreeRange)
                {
                    nodes = nodes.Where(gpo => gpo.Distance2D(Core.Player.Location) < Radius);
                }
                else
                {
                    if (HotSpots != null)
                    {
                        nodes = nodes.OrderBy(gpo => gpo.Location.Distance2D(Core.Player.Location)).Where(gpo => HotSpots.CurrentOrDefault.WithinHotSpot2D(gpo.Location));
                    }
                }

                if (GatherObjects != null)
                {
                    Node = nodes.OrderBy(gpo => GatherObjects.FindIndex(i => string.Equals(gpo.EnglishName, i, StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault(gpo => GatherObjects.Contains(gpo.EnglishName, StringComparer.InvariantCultureIgnoreCase));
                }
                else
                {
                    Node = nodes.FirstOrDefault();
                }

                if (Node == null)
                {
                    if (HotSpots != null)
                    {
                        if (GatherStrategy == GatherStrategy.GatherOrCollect && retryCenterHotspot)
                        {
                            await Behaviors.MoveTo(HotSpots.CurrentOrDefault, true, (uint)MountId, Radius, NavHeight, HotSpots.CurrentOrDefault.Name, LogFlight);

                            Logging.Write(Colors.PaleVioletRed, "GatherCollectable: Could not find any nodes, trying again from center of hotspot.");

                            retryCenterHotspot = false;
                            continue;
                        }

                        if (!await ChangeHotSpot())
                        {
                            retryCenterHotspot = false;
                            continue;
                        }
                    }

                    if (FreeRange && !FreeRangeConditional())
                    {
                        await Coroutine.Yield();
                        isDone = true;
                    }

                    return false;
                }

                var entry = Blacklist.GetEntry(Node.ObjectId);
                if (entry != null && entry.Flags.HasFlag(BlacklistFlags.Interact))
                {
                    Logging.Write(Colors.PaleVioletRed, "Node on blacklist, waiting until we move out of range or it clears.");

                    if (await Coroutine.Wait(entry.Length, () => entry.IsFinished || Node.Location.Distance2D(Core.Player.Location) > Radius))
                    {
                        if (!entry.IsFinished)
                        {
                            Node = null;
                            Logging.Write(Colors.Chartreuse, "GatherCollectable: Node Reset, Reason: Ran out of range");
                            return false;
                        }
                    }

                    Logging.Write(Colors.Chartreuse, "GatherCollectable: Node removed from blacklist.");
                }

                Logging.Write(Colors.Chartreuse, "GatherCollectable: Node set -> " + Node);

                return true;
            }
        }

        private async Task<bool> MoveToGatherSpot()
        {
            var result = await GatherSpot.MoveToSpot(this);
            return result;
        }

        private async Task<bool> MoveFromGatherSpot()
        {
            var result = await GatherSpot.MoveFromSpot(this);

            return result;
        }

        private async Task<bool> BeforeGather()
        {
            if (Poi.Current.Unit != Node)
            {
                Poi.Current = new Poi(Node, PoiType.Gather);
            }

            //TODO: Fix the logic so it is easier to read
            if (Core.Player.CurrentGP >= AdjustedWaitForGp)
            {
                return true;
            }

            var eorzeaMinutesTillDespawn = int.MaxValue;
            if (this.IsUnspoiled())
            {
                eorzeaMinutesTillDespawn = 55 - WorldManager.EorzaTime.Minute;
            }

            if (this.IsEphemeral())
            {
                var hoursFromNow = WorldManager.EorzaTime.AddHours(4);
                var rounded = new DateTime(
                    hoursFromNow.Year,
                    hoursFromNow.Month,
                    hoursFromNow.Day,
                    hoursFromNow.Hour - (hoursFromNow.Hour % 4),
                    0,
                    0);

                eorzeaMinutesTillDespawn = (int)(rounded - WorldManager.EorzaTime).TotalMinutes;
            }

            var realSecondsTillDespawn = eorzeaMinutesTillDespawn * 35 / 12;
            var realSecondsTillStartGathering = realSecondsTillDespawn - gatherRotation.Attributes.RequiredTimeInSeconds;

            if (realSecondsTillStartGathering < 1)
            {
                if (gatherRotation.ForceGatherIfMissingGpOrTime)
                {
                    return true;
                }

                Logging.Write("Not enough time to gather");
                // isDone = true;
                return true;
            }

            var ticksTillStartGathering = realSecondsTillStartGathering / 3;

            var gp = Math.Min(Core.Player.CurrentGP + ticksTillStartGathering * 5, Core.Player.MaxGP);

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
                // isDone = true;
                return true;
            }

            if (gatherRotation.ForceGatherIfMissingGpOrTime)
            {
                return true;
            }

            if (gp >= AdjustedWaitForGp && !CordialTime.HasFlag(CordialTime.BeforeGather))
            {
                return await WaitForGpRegain();
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
            eorzeaMinutesTillDespawn = int.MaxValue;
            if (this.IsUnspoiled())
            {
                eorzeaMinutesTillDespawn = 55 - WorldManager.EorzaTime.Minute;
            }

            if (this.IsEphemeral())
            {
                var hoursFromNow = WorldManager.EorzaTime.AddHours(4);
                var rounded = new DateTime(
                    hoursFromNow.Year,
                    hoursFromNow.Month,
                    hoursFromNow.Day,
                    hoursFromNow.Hour - (hoursFromNow.Hour % 4),
                    0,
                    0);

                eorzeaMinutesTillDespawn = (int)(rounded - WorldManager.EorzaTime).TotalMinutes;
            }

            realSecondsTillDespawn = eorzeaMinutesTillDespawn * 35 / 12;
            realSecondsTillStartGathering = realSecondsTillDespawn - gatherRotation.Attributes.RequiredTimeInSeconds;

            if (realSecondsTillStartGathering < 1)
            {
                if (gatherRotation.ForceGatherIfMissingGpOrTime)
                {
                    return true;
                }

                Logging.Write("Not enough GP to gather");
                // isDone = true;
                return true;
            }

            ticksTillStartGathering = realSecondsTillStartGathering / 3;

            gp = Math.Min(Core.Player.CurrentGP + ticksTillStartGathering * 5, Core.Player.MaxGP);

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
            var eorzeaMinutesTillDespawn = int.MaxValue;
            if (this.IsUnspoiled())
            {
                eorzeaMinutesTillDespawn = 55 - WorldManager.EorzaTime.Minute;
            }

            if (this.IsEphemeral())
            {
                var hoursFromNow = WorldManager.EorzaTime.AddHours(4);
                var rounded = new DateTime(
                    hoursFromNow.Year,
                    hoursFromNow.Month,
                    hoursFromNow.Day,
                    hoursFromNow.Hour - (hoursFromNow.Hour % 4),
                    0,
                    0);

                eorzeaMinutesTillDespawn = (int)(rounded - WorldManager.EorzaTime).TotalMinutes;
            }

            var realSecondsTillDespawn = eorzeaMinutesTillDespawn * 35 / 12;
            var realSecondsTillStartGathering = realSecondsTillDespawn - gatherRotation.Attributes.RequiredTimeInSeconds;

            if (realSecondsTillStartGathering < 1)
            {
                if (gatherRotation.ForceGatherIfMissingGpOrTime)
                {
                    return true;
                }

                Logging.Write("Not enough time to gather");
                // isDone = true;
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
            if (CordialTime.HasFlag(CordialTime.AfterGather))
            {
                if (CordialType == CordialType.Auto)
                {
                    if (Core.Player.MaxGP - Core.Player.CurrentGP > 550)
                    {
                        if (await UseCordial(CordialType.HiCordial))
                        {
                            return true;
                        }
                    }

                    if (Core.Player.MaxGP - Core.Player.CurrentGP > 390)
                    {
                        if (await UseCordial(CordialType.Cordial))
                        {
                            return true;
                        }
                    }
                }

                if (CordialType == CordialType.HiCordial)
                {
                    if (Core.Player.MaxGP - Core.Player.CurrentGP > 430)
                    {
                        if (await UseCordial(CordialType.HiCordial))
                        {
                            return true;
                        }

                        if (await UseCordial(CordialType.Cordial))
                        {
                            return true;
                        }
                    }
                }

                if (CordialType == CordialType.Cordial && Core.Player.MaxGP - Core.Player.CurrentGP > 330)
                {
                    if (await UseCordial(CordialType.Cordial))
                    {
                        return true;
                    }
                }
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
            if (CordialSpellData.Cooldown.TotalSeconds < maxTimeoutSeconds)
            {
                var cordial =
                    InventoryManager.FilledSlots.FirstOrDefault(
                        slot => slot.Item.Id == (uint)cordialType);

                if (cordial != null)
                {
                    Logging.Write(
                        "Using Cordial -> Waiting (sec): " + maxTimeoutSeconds + " CurrentGP: " + Core.Player.CurrentGP);
                    if (await Coroutine.Wait(
                        TimeSpan.FromSeconds(maxTimeoutSeconds),
                        () =>
                        {
                            if (Core.Player.IsMounted && CordialSpellData.Cooldown.TotalSeconds < 2)
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
                else
                {
                    Logging.Write(Colors.Chartreuse, "No Cordial avilable, buy more " + cordialType);
                }
            }

            return false;
        }

        private async Task<bool> Gather()
        {
            if (!Blacklist.Contains(Poi.Current.Unit, BlacklistFlags.Interact))
            {
                var timeToBlacklist = GatherStrategy == GatherStrategy.TouchAndGo
                                          ? TimeSpan.FromSeconds(15)
                                          : TimeSpan.FromSeconds(
                                              Math.Max(gatherRotation.Attributes.RequiredTimeInSeconds + 6, 30));
                Blacklist.Add(Poi.Current.Unit, BlacklistFlags.Interact, timeToBlacklist, "Blacklisting node so that we don't retarget -> " + Poi.Current.Unit);
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
                    //SetFallbackGatherSpot(Node.Location, true);

                    await MoveToGatherSpot();
                }
            }

            if (!GatheringManager.WindowOpen)
            {
                OnResetCachedDone();
                return true;
            }

            if (!await ResolveGatherItem())
            {
                ResetInternal();
                return false;
            }

            CheckForGatherRotationOverride();

            await gatherRotation.Prepare(this);
            await gatherRotation.ExecuteRotation(this);
            await gatherRotation.Gather(this);

            await Coroutine.Wait(6000, () => !Node.CanGather);

            if (!object.ReferenceEquals(gatherRotation, initialGatherRotation))
            {
                gatherRotation = initialGatherRotation;
                Logging.Write(Colors.Chartreuse, "GatherCollectable: Rotation reset -> " + GatherRotation);
            }

            Poi.Clear("Gather Complete, Node is gone!");

            return true;
        }

        internal async Task<bool> Cast(uint id)
        {
            return await Actions.Cast(id, SpellDelay);
        }

        internal async Task<bool> Cast(Ability id)
        {
            return await Actions.Cast(id, SpellDelay);
        }

        internal async Task<bool> CastAura(uint spellId, int auraId = -1)
        {
            return await Actions.CastAura(spellId, SpellDelay, auraId);
        }

        internal async Task<bool> CastAura(Ability ability, AbilityAura auraId = AbilityAura.None)
        {
            return await Actions.CastAura(ability, SpellDelay, auraId);
        }

        internal async Task<bool> ResolveGatherItem()
        {
            GatherItem = null;
            CollectableItem = null;
            var windowItems = GatheringManager.GatheringWindowItems;

            // TODO: move method to common so we use it on fish too
            if (InventoryItemCount() >= 100)
            {
                if (SetGatherItemByItemName(windowItems.OrderByDescending(i => i.SlotIndex)
                        .Where(i => i.IsFilled && !i.IsUnknown && i.ItemId < 20).ToArray()))
                {
                    return true;
                }
            }

            if (DiscoverUnknowns)
            {
                var items =
                    new[] { 0U, 1U, 2U, 3U, 4U, 5U, 6U, 7U }.Select(GatheringManager.GetGatheringItemByIndex)
                        .ToArray();

                GatherItem = items.FirstOrDefault(i => i.IsUnknown && i.Amount > 0);

                if (GatherItem != null)
                {
                    return true;
                }
            }

            if (Collectables != null && Collectables.Count > 0)
            {
                foreach (var collectable in Collectables)
                {
                    GatherItem =
                        windowItems.FirstOrDefault(
                            i =>
                            i.IsFilled && !i.IsUnknown
                            && string.Equals(
                                collectable.Name,
                                i.ItemData.EnglishName,
                                StringComparison.InvariantCultureIgnoreCase));

                    if (GatherItem != null)
                    {
                        CollectableItem = collectable;
                        return true;
                    }
                }
            }

            if (ItemNames != null && ItemNames.Count > 0)
            {
                if (SetGatherItemByItemName(windowItems))
                {
                    return true;
                }
            }

            if (Slot > -1 && Slot < 8)
            {
                GatherItem = GatheringManager.GetGatheringItemByIndex((uint)Slot);
            }

            if (GatherItem == null && (!AlwaysGather || GatherStrategy == GatherStrategy.TouchAndGo))
            {
                Poi.Clear("Skipping this node, no items we want to gather.");
                var window = RaptureAtkUnitManager.GetWindowByName("Gathering");

                while (window.IsValid)
                {
                    window.SendAction(1, 3, 0xFFFFFFFF);
                    await Coroutine.Yield();
                }

                return false;
            }

            if (GatherItem != null)
            {
                return true;
            }

            GatherItem =
                windowItems.Where(i => i.IsFilled && !i.IsUnknown)
                    .OrderByDescending(i => i.ItemId)
                    .FirstOrDefault(i => i.ItemId < 20) // Try to gather cluster/crystal/shard
                ?? windowItems.FirstOrDefault(i => !i.ItemData.Unique && !i.ItemData.Untradeable && i.ItemData.ItemCount() > 0) // Try to collect items you have that stack
                ?? windowItems.Where(i => !i.ItemData.Unique && !i.ItemData.Untradeable).OrderByDescending(i => i.SlotIndex).First(); // Take last item that is not unique or untradeable

            Logging.Write(Colors.Chartreuse, "GatherCollectable: could not find item by slot or name, gathering " + GatherItem.ItemData + " instead.");

            return true;
        }

        private void SetFallbackGatherSpot(Vector3 location, bool useMesh)
        {
            switch (DefaultGatherSpotType)
            {
                case GatherSpotType.StealthApproachGatherSpot:
                case GatherSpotType.StealthGatherSpot:
                    GatherSpot = new StealthGatherSpot { NodeLocation = location, UseMesh = useMesh };
                    break;
                case GatherSpotType.GatherSpot:
                default:
                    GatherSpot = new GatherSpot { NodeLocation = location, UseMesh = useMesh };
                    break;
            }
        }

        private bool SetGatherItemByItemName(ICollection<GatheringItem> windowItems)
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

            return false;
        }

        private void CheckForGatherRotationOverride()
        {
            if (!gatherRotation.CanOverride)
            {
                return;
            }

            var rotationAndTypes = Rotations.Where(kvp => !object.ReferenceEquals(kvp.Value, gatherRotation))
                .Select(
                    r => new
                             {
                                 Rotation = r.Value,
                                 OverrideValue = r.Value.ShouldOverrideSelectedGatheringRotation(this)
                             })
                .Where(r => r.OverrideValue > -1)
                .OrderByDescending(r => r.OverrideValue).ToArray();

            var rotation = rotationAndTypes.FirstOrDefault();

            if (rotation == null)
            {
                return;
            }

            Logging.Write(
                Colors.Chartreuse,
                "GatherCollectable: Rotation Override -> Old: "
                + gatherRotation.Attributes.Name
                + " , New: "
                + rotation.Rotation.Attributes.Name);

            gatherRotation = rotation.Rotation;
        }

        private bool FreeRangeConditional()
        {
            if (freeRangeConditionFunc == null)
            {
                freeRangeConditionFunc = ScriptManager.GetCondition(FreeRangeCondition);
            }

            return freeRangeConditionFunc();
        }

        private int InventoryItemCount()
        {
            return InventoryManager.FilledSlots.Count(c => c.BagId != InventoryBagId.KeyItems);
        }

        internal bool IsEphemeral()
        {
            return Node.EnglishName.IndexOf("ephemeral", StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        internal bool IsUnspoiled()
        {
            return Node.EnglishName.IndexOf("unspoiled", StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}
