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

    using Clio.XmlEngine;

    using Exbuddy.OrderBotTags;

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

        private IGatheringRotation gatherRotation;

        private ushort gatherRotationGp;

        private byte gatherRotationTime;

        private IGatherSpot gatherSpot;

        private GatheringPointObject node;

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

        [DefaultValue(CordialTime.BeforeGather)]
        [XmlElement("CordialTime")]
        public CordialTime CordialTime { get; set; }

        [DefaultValue(CordialType.Auto)]
        [XmlElement("CordialType")]
        public CordialType CordialType { get; set; }

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
            isDone = false;
            gatherSpot = null;
            gatherRotation = null;
            gatherRotationGp = 0;
            gatherRotationTime = 0;
            node = null;
        }

        protected override Composite CreateBehavior()
        {
            // Had to add null check for node.
            return
                new PrioritySelector(
                    new Decorator(
                        ret => gatherRotation == null,
                        new ActionRunCoroutine(ctx => ResolveGatherRotation())),
                    new Decorator(
                        ret => node == null,
                        new Sequence(
                            new ActionRunCoroutine(ctx => FindNode()),
                            new Action(r => MovementManager.SetFacing2D(node.Location)))),
                    new Decorator(
                        ret => node != null && gatherSpot == null,
                        new ActionRunCoroutine(ctx => FindGatherSpot())),
                    new Decorator(
                        ret =>
                        node != null && gatherSpot != null && node.Location.Distance3D(Core.Player.Location) > Distance,
                        new ActionRunCoroutine(ctx => MoveToGatherSpot())),
                    new Decorator(
                        ret =>
                        node != null && gatherSpot != null &&  node.CanGather
                        && node.Location.Distance3D(Core.Player.Location) <= Distance,
                        new Sequence(
                            new ActionRunCoroutine(ctx => BeforeGather()),
                            new ActionRunCoroutine(ctx => Gather()),
                            new ActionRunCoroutine(ctx => AfterGather()))),
                    new Decorator(
                        ret => node != null && gatherSpot != null && !node.CanGather,
                        new ActionRunCoroutine(ctx => MoveFromGatherSpot())));
        }

        private static Dictionary<string, Type> LoadRotationTypes()
        {
            try
            {
                var types =
                    Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(t => !t.IsAbstract && typeof(IGatheringRotation).IsAssignableFrom(t))
                        .ToArray();

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
            catch
            {
                Logging.Write("Unable to get types, Loading Known Rotations.");
            }

            return LoadKnownRotationTypes();
        }

        private static Dictionary<string, Type> LoadKnownRotationTypes()
        {
            return new Dictionary<string, Type>
                       {
                           { "Default", typeof(DefaultGatheringRotation) },
                           { "DefaultCollect", typeof(DefaultCollectGatheringRotation) },
                           { "Collect470", typeof(Collect470GatheringRotation) },
                           { "Collect450", typeof(Collect450GatheringRotation) },
                           { "Collect550", typeof(Collect550GatheringRotation) },
                           { "Collect570", typeof(Collect550GatheringRotation) }
                       };
        } 

        private async Task<bool> ResolveGatherRotation()
        {
            Type rotationType;
            if (!Rotations.TryGetValue(GatherRotation, out rotationType))
            {
                rotationType = typeof(DefaultCollectGatheringRotation);
                Logging.Write("Could not find rotation, using DefaultCollect instead.");
            }

            gatherRotation = rotationType.CreateInstance<IGatheringRotation>();
            gatherRotationGp =
                rotationType.GetCustomAttributePropertyValue<GatheringRotationAttribute, ushort>(p => p.RequiredGp);
            gatherRotationTime =
                rotationType.GetCustomAttributePropertyValue<GatheringRotationAttribute, byte>(
                    p => p.RequiredTimeInSeconds);

            Logging.Write("Using rotation: " + rotationType.GetCustomAttributePropertyValue<GatheringRotationAttribute, string>(p => p.Name, rotationType.Name.Replace("GatheringRotation", string.Empty)));

            return true;
        }

        private async Task<bool> FindGatherSpot()
        {
            if (GatherSpots != null)
            {
                gatherSpot = GatherSpots.FirstOrDefault(gs => gs != null && gs.IsMatch);
            }

            // Either GatherSpots is null or there are no matches, use fallback
            if (gatherSpot == null)
            {
                gatherSpot = new GatherSpot { NodeLocation = node.Location, UseMesh = true };
            }

            return true;
        }

        private async Task<bool> FindNode()
        {
            if (GatherObjects != null)
            {
                node =
                    GameObjectManager.GetObjectsOfType<GatheringPointObject>()
                        .OrderBy(gpo => GatherObjects.FindIndex(i => string.Equals(gpo.EnglishName, i, StringComparison.InvariantCultureIgnoreCase)))
                        .FirstOrDefault(
                            gpo => GatherObjects.Contains(gpo.EnglishName, StringComparer.InvariantCultureIgnoreCase)
                            && gpo.CanGather);
            }
            else
            {
                node = GameObjectManager.GetObjectsOfType<GatheringPointObject>()
                    .FirstOrDefault(gpo => gpo.CanGather);
            }

            if (node == null)
            {
                return false;
            }

            return true;
        }

        private async Task<bool> MoveToGatherSpot()
        {
            var result =
                await 
                gatherSpot.MoveToSpot(
                    () => Actions.CastAura(Ability.Stealth, AbilityAura.Stealth),
                    node.Location,
                    (uint)MountId,
                    Radius,
                    NavHeight,
                    "Gather Spot",
                    LogFlight);
            return result;
        }

        private async Task<bool> MoveFromGatherSpot()
        {
            var result = await gatherSpot.MoveFromSpot();

            isDone = true;
            return result;
        }

        private async Task<bool> BeforeGather()
        {
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

            if (gp >= AdjustedWaitForGp || !CordialTime.HasFlag(CordialTime.BeforeGather))
            {
                return await WaitForGpRegain();
            }
            if (realSecondsTillStartGathering < cordialSpellData.AdjustedCooldown.Seconds)
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
 
            return true;
        }

        private async Task<bool> UseCordial(CordialType cordialType, int maxTimeoutSeconds = 5)
        {
            if (cordialSpellData.AdjustedCooldown.Seconds < maxTimeoutSeconds)
            {
                var cordial =
                    InventoryManager.FilledSlots.FirstOrDefault(
                        slot => slot.Item.Id == (uint)cordialType);

                if (cordial != null)
                {
                    Logging.Write("Using Cordial -> Waiting (sec): " + maxTimeoutSeconds + " CurrentGP: " + Core.Player.CurrentGP);
                    if (await Coroutine.Wait(TimeSpan.FromSeconds(maxTimeoutSeconds), () => cordial.CanUse(Core.Player)))
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

            Poi.Current = new Poi(node, PoiType.Gather);
            Poi.Current.Unit.Interact();

            if (!await Coroutine.Wait(6000, () => GatheringManager.WindowOpen))
            {
                Logging.Write("Gathering Window didn't open: Re-attempting to move into place.");
                gatherSpot = new GatherSpot { NodeLocation = node.Location, UseMesh = true };
                return false;
            }

            await Coroutine.Sleep(2200);

            var item = await gatherRotation.Prepare((uint)this.Slot);
            await gatherRotation.ExecuteRotation(item);
            await gatherRotation.Gather((uint)this.Slot);

            Poi.Clear("Gather Complete!");

            return true;
        }
    }
}
