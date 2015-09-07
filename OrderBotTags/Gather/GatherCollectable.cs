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

    using ExBuddy.OrderBotTags.Common;
    using ExBuddy.OrderBotTags.Gather.Rotations;
    using ExBuddy.OrderBotTags.Navigation;

    using ff14bot;
    using ff14bot.Behavior;
    using ff14bot.Enums;
    using ff14bot.Helpers;
    using ff14bot.Managers;
    using ff14bot.Navigation;
    using ff14bot.NeoProfiles;
    using ff14bot.Objects;

    using TreeSharp;

    [XmlElement("GatherCollectable")]
    public sealed class GatherCollectableTag : FlightVars
    {
        internal static readonly Dictionary<string, IGatheringRotation> Rotations;

        internal static readonly SpellData CordialSpellData = DataManager.GetItem((uint)CordialType.Cordial).BackingAction;

        private bool isDone;

        private int loopCount;

        private Func<bool> freeRangeConditionFunc;

        private IGatheringRotation initialGatherRotation;

        private IGatheringRotation gatherRotation;

        internal bool GatherItemIsFallback;

        internal IGatherSpot GatherSpot;

        internal GatheringPointObject Node;

        internal GatheringItem GatherItem;

        internal Collectable CollectableItem;

        internal int NodesGatheredAtMaxGp;

        private Func<bool> whileFunc;

        private DateTime startTime;

        private BotEvent cleanup;

        private FlightEnabledNavigator navigator;

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
                var requiredGp = gatherRotation == null ? 0 : gatherRotation.Attributes.RequiredGp;

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

        [XmlElement("GatheringSkillOrder")]
        public GatheringSkillOrder GatheringSkillOrder { get; set; }

        // I want this to be an attribute, but for backwards compatibilty, we will use element
        [DefaultValue(-1)]
        [XmlElement("Slot")]
        public int Slot { get; set; }

        // Backwards compatibility
        [XmlElement("GatherObject")]
        public string GatherObject { get; set; }

        [XmlElement("GatherObjects")]
        public List<string> GatherObjects { get; set; }

        [XmlAttribute("DisableRotationOverride")]
        public bool DisableRotationOverride { get; set; }

        // Maybe this should be an attribute?
        [DefaultValue("RegularNode")]
        [XmlElement("GatherRotation")]
        public string GatherRotation { get; set; }

        [XmlElement("GatherSpots")]
        public List<StealthApproachGatherSpot> GatherSpots { get; set; }

        [DefaultValue(GatherIncrease.Auto)]
        [XmlAttribute("GatherIncrease")]
        public GatherIncrease GatherIncrease { get; set; }

        [DefaultValue(GatherStrategy.GatherOrCollect)]
        [XmlAttribute("GatherStrategy")]
        public GatherStrategy GatherStrategy { get; set; }

        [XmlElement("HotSpots")]
        public IndexedList<HotSpot> HotSpots { get; set; }

        [XmlElement("Collectables")]
        public List<Collectable> Collectables { get; set; }

        [XmlElement("ItemNames")]
        public List<string> ItemNames { get; set; }

        [DefaultValue(3.1f)]
        [XmlAttribute("Distance")]
        public float Distance { get; set; }

        [XmlAttribute("SkipWindowDelay")]
        public uint SkipWindowDelay { get; set; }

        [DefaultValue(2500)]
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

        private bool HandleCondition()
        {
            if (whileFunc == null)
            {
                whileFunc = ScriptManager.GetCondition(While);
            }

            // If statement is true, return false so we can continue the routine
            if (whileFunc())
            {
                return false;
            }

            isDone = true;
            return true;
        }

        protected override void OnResetCachedDone()
        {
            if (!isDone)
            {
                Logging.Write(Colors.Chartreuse, "GatherCollectable: Resetting.");
            }

            isDone = false;
            loopCount = 0;
            NodesGatheredAtMaxGp = 0;
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
            SpellDelay = SpellDelay < 100 ? 100 : SpellDelay;
            WindowDelay = WindowDelay < 500 ? 500 : WindowDelay;

            if (Distance > 3.5f)
            {
                TreeRoot.Stop("Using a distance of greater than 3.5 is not supported, change the value and restart the profile.");
            }

            if (HotSpots != null)
            {
                HotSpots.IsCyclic = Loops < 1;
            }

            // backwards compatibility
            if (GatherObjects == null && !string.IsNullOrWhiteSpace(GatherObject))
            {
                GatherObjects = new List<string> { GatherObject };
            }

            startTime = DateTime.Now;

            //navigator = new FlightEnabledNavigator(Navigator.NavigationProvider);

            cleanup = bot =>
            {
                DoCleanup();
                TreeRoot.OnStop -= cleanup;
            };

            TreeRoot.OnStop += cleanup;
        }

        protected override void OnDone()
        {

            TreeRoot.OnStop -= cleanup;
            DoCleanup();
        }

        private void DoCleanup()
        {
            if (navigator != null)
            {
                navigator.Dispose();
            }
        }

        protected override Composite CreateBehavior()
        {
            return new ActionRunCoroutine(ctx => Main());
        }

        private async Task<bool> Main()
        {
            await CommonTasks.HandleLoading();

            return HandleCondition()
                || await CastTruth()
                || HandleReset()
                || await MoveToHotSpot()
                || await FindNode()
                || FindGatherSpot()
                || ResolveGatherRotation()
                || await MoveToGatherSpot()
                || await GatherSequence()
                || (await MoveFromGatherSpot() && ResetOrDone());
        }

        private async Task<bool> GatherSequence()
        {
            if (!Node.CanGather || !(Node.Location.Distance3D(Core.Player.Location) <= Distance))
            {
                return false;
            }

            return await BeforeGather() && await Gather() && await AfterGather();
        }

        private bool HandleReset()
        {
            if (Node == null || (Node.IsValid && (!FreeRange || !(Node.Location.Distance3D(Core.Player.Location) > Radius))))
            {
                return false;
            }

            OnResetCachedDone();
            return true;
        }

        private async Task<bool> MoveToHotSpot()
        {
            if (HotSpots != null && !HotSpots.CurrentOrDefault.WithinHotSpot2D(Core.Player.Location))
            {
                //return lets try not caring if we succeed on the move
                    await
                    Behaviors.MoveTo(
                        HotSpots.CurrentOrDefault,
                        true,
                        (uint)MountId,
                        HotSpots.CurrentOrDefault.Radius * 0.75f,
                        HotSpots.CurrentOrDefault.Name);

                return true;
            }

            return false;
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
                            typeof(RegularNodeGatheringRotation),
                            typeof(UnspoiledGatheringRotation) ,
                            typeof(DefaultCollectGatheringRotation),
                            typeof(Collect115GatheringRotation),
                            typeof(Collect240DynamicGatheringRotation),
                            typeof(Collect345GatheringRotation),
                            typeof(Collect450GatheringRotation),
                            typeof(Collect470GatheringRotation),
                            typeof(Collect550GatheringRotation),
                            typeof(Collect570GatheringRotation),
                            typeof(DiscoverUnknownsGatheringRotation),
                            typeof(ElementalGatheringRotation),
                            typeof(TopsoilGatheringRotation),
                            typeof(MapGatheringRotation),
                            typeof(SmartQualityGatheringRotation),
                            typeof(SmartYieldGatheringRotation)
                       };
        }

        private bool ResolveGatherRotation()
        {
            if (gatherRotation != null)
            {
                return false;
            }

            if (GatheringSkillOrder != null && GatheringSkillOrder.GatheringSkills.Count > 0)
            {
                initialGatherRotation = gatherRotation = new GatheringSkillOrderGatheringRotation();

                Logging.Write(Colors.Chartreuse, "GatherCollectable: Using rotation -> " + gatherRotation.Attributes.Name);
            }

            IGatheringRotation rotation;
            if (!Rotations.TryGetValue(GatherRotation, out rotation))
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (!Rotations.TryGetValue("RegularNode", out rotation))
                {
                    rotation = new RegularNodeGatheringRotation();
                }
                else
                {
                    rotation = Rotations["RegularNode"];
                }

                Logging.Write(Colors.PaleVioletRed, "GatherCollectable: Could not find rotation, using RegularNode instead.");
            }

            initialGatherRotation = gatherRotation = rotation;

            Logging.Write(Colors.Chartreuse, "GatherCollectable: Using rotation -> " + rotation.Attributes.Name);

            return true;
        }

        private async Task<bool> CastTruth()
        {
            if (MovementManager.IsFlying
                || Core.Player.ClassLevel < 46
                || Core.Player.HasAura((int)
                        (Core.Player.CurrentJob == ClassJobType.Miner
                             ? AbilityAura.TruthOfMountains
                             : AbilityAura.TruthOfForests)))
            {
                return false;
            }

            while (Core.Player.IsMounted)
            {
                await CommonTasks.StopAndDismount();
                await Coroutine.Yield();
            }

            return await
                CastAura(
                    Ability.Truth,
                    Core.Player.CurrentJob == ClassJobType.Miner ? AbilityAura.TruthOfMountains : AbilityAura.TruthOfForests);
        }

        private bool ResetOrDone()
        {
            if (HotSpots == null || HotSpots.Count == 0)
            {
                isDone = true;
            }
            else
            {
                ResetInternal();
            }

            return true;
        }

        private bool ChangeHotSpot()
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
                    Logging.Write(Colors.Chartreuse, "GatherCollectable: Finished {0} of {1} loops.", ++loopCount, Loops);

                    // If finished all loops, otherwise just incrementing loop count
                    if (loopCount == Loops)
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

        private bool FindGatherSpot()
        {
            if (GatherSpot != null)
            {
                return false;
            }

            if (GatherSpots != null && Node.Location.Distance3D(Core.Player.Location) > Distance)
            {
                GatherSpot = GatherSpots.OrderBy(gs => gs.NodeLocation.Distance3D(Node.Location)).FirstOrDefault(gs => gs.NodeLocation.Distance3D(Node.Location) <= Distance);
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
            if (Node != null)
            {
                return false;
            }

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

                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
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
                        var myLocation = Core.Player.Location;
                        if (GatherStrategy == GatherStrategy.GatherOrCollect && retryCenterHotspot
                            && GameObjectManager.GameObjects.Select(o => o.Location.Distance2D(myLocation))
                                   .OrderByDescending(o => o).FirstOrDefault() <= myLocation.Distance2D(HotSpots.CurrentOrDefault) + HotSpots.CurrentOrDefault.Radius)
                        {
                            Logging.Write(Colors.PaleVioletRed, "GatherCollectable: Could not find any nodes and can not confirm hotspot is empty via object detection, trying again from center of hotspot.");
                            await Behaviors.MoveTo(HotSpots.CurrentOrDefault, true, (uint)MountId, Radius, HotSpots.CurrentOrDefault.Name);
                            
                            retryCenterHotspot = false;
                            await Coroutine.Yield();
                            continue;
                        }

                        if (!ChangeHotSpot())
                        {
                            retryCenterHotspot = false;
                            await Coroutine.Yield();
                            continue;
                        }
                    }

                    if (FreeRange && !FreeRangeConditional())
                    {
                        await Coroutine.Yield();
                        isDone = true;
                        return true;
                    }

                    return true;
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

                if (HotSpots == null)
                {
                    MovementManager.SetFacing2D(Node.Location);
                }

                return true;
            }
        }

        private async Task<bool> MoveToGatherSpot()
        {
            if (FreeRange || !(Node.Location.Distance3D(Core.Player.Location) > Distance))
            {
                return false;
            }

            return await GatherSpot.MoveToSpot(this);
        }

        private async Task<bool> MoveFromGatherSpot()
        {
            if (Node.CanGather)
            {
                return false;
            }

            return await GatherSpot.MoveFromSpot(this);
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
            if (IsUnspoiled())
            {
                eorzeaMinutesTillDespawn = 55 - WorldManager.EorzaTime.Minute;
            }

            if (IsEphemeral())
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
                if (gatherRotation.ShouldForceGather)
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

                if (gatherRotation.ShouldForceGather)
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

            if (gatherRotation.ShouldForceGather)
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
                    if (gatherRotation.ShouldForceGather)
                    {
                        return true;
                    }

                    return await WaitForGpRegain();
                }
            }

            // Recalculate: could have no time left at this point
            eorzeaMinutesTillDespawn = int.MaxValue;
            if (IsUnspoiled())
            {
                eorzeaMinutesTillDespawn = 55 - WorldManager.EorzaTime.Minute;
            }

            if (IsEphemeral())
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
                if (gatherRotation.ShouldForceGather)
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
                    if (gatherRotation.ShouldForceGather)
                    {
                        return true;
                    }

                    return await WaitForGpRegain();
                }
            }

            if (gatherRotation.ShouldForceGather)
            {
                return true;
            }

            return await WaitForGpRegain();
        }

        private async Task<bool> WaitForGpRegain()
        {
            var eorzeaMinutesTillDespawn = int.MaxValue;
            if (IsUnspoiled())
            {
                eorzeaMinutesTillDespawn = 55 - WorldManager.EorzaTime.Minute;
            }

            if (IsEphemeral())
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
                if (gatherRotation.ShouldForceGather)
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
            Poi.Clear("Gather Complete, Node is gone!");

            if (Core.Player.CurrentGP >= Core.Player.MaxGP - 30)
            {
                NodesGatheredAtMaxGp++;
            }
            else
            {
                NodesGatheredAtMaxGp = 0;
            }

            if (!object.ReferenceEquals(gatherRotation, initialGatherRotation))
            {
                gatherRotation = initialGatherRotation;
                Logging.Write(Colors.Chartreuse, "GatherCollectable: Rotation reset -> " + GatherRotation);
            }

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
                //TODO: check 
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

        private async Task<bool> InteractWithNode()
        {
            var attempts = 0;
            while (attempts < 3 && !GatheringManager.WindowOpen)
            {
                while (MovementManager.IsFlying)
                {
                    Navigator.Stop();
                    Actionmanager.Dismount();
                    await Coroutine.Yield();
                }

                Poi.Current.Unit.Interact();

                if (await Coroutine.Wait(WindowDelay, () => GatheringManager.WindowOpen))
                {
                    continue;
                }

                if (FreeRange)
                {
                    Logging.Write("Gathering Window didn't open: Retrying. " + ++attempts);
                    continue;
                }

                Logging.Write("Gathering Window didn't open: Re-attempting to move into place. " + ++attempts);
                //SetFallbackGatherSpot(Node.Location, true);

                await MoveToGatherSpot();
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

            return true;
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

            return await InteractWithNode()
                && await gatherRotation.Prepare(this)
                && await gatherRotation.ExecuteRotation(this)
                && await gatherRotation.Gather(this)
                && await Coroutine.Wait(6000, () => !Node.CanGather)
                && await WaitForGatherWindowToClose();
        }

        private async Task<bool> WaitForGatherWindowToClose()
        {
            while (GatheringManager.WindowOpen)
            {
                await Coroutine.Yield();
            }

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
            var previousGatherItem = GatherItem;
            GatherItemIsFallback = false;
            GatherItem = null;
            CollectableItem = null;
            var windowItems = GatheringManager.GatheringWindowItems;

            // TODO: move method to common so we use it on fish too
            if (InventoryItemCount() >= 100)
            {
                if (ItemNames != null && ItemNames.Count > 0)
                {
                    if (SetGatherItemByItemName(windowItems.OrderByDescending(i => i.SlotIndex)
                                           .Where(i => i.IsFilled && !i.IsUnknown && i.ItemId < 20).ToArray()))
                    {
                        return true;
                    }
                }

                GatherItem =
                    windowItems.Where(i => i.IsFilled && !i.IsUnknown)
                        .OrderByDescending(i => i.ItemId)
                        .FirstOrDefault(i => i.ItemId < 20);

                if (GatherItem != null)
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

                if (SkipWindowDelay > 0)
                {
                    await Coroutine.Sleep((int)SkipWindowDelay);
                }

                await CloseGatheringWindow();

                return false;
            }

            if (GatherItem != null)
            {
                return true;
            }

            GatherItemIsFallback = true;

            GatherItem =
                windowItems.Where(i => i.IsFilled && !i.IsUnknown)
                    .OrderByDescending(i => i.ItemId)
                    .FirstOrDefault(i => i.ItemId < 20) // Try to gather cluster/crystal/shard
                ?? windowItems.FirstOrDefault(i => !i.ItemData.Unique && !i.ItemData.Untradeable && i.ItemData.ItemCount() > 0) // Try to collect items you have that stack
                ?? windowItems.Where(i => !i.ItemData.Unique && !i.ItemData.Untradeable).OrderByDescending(i => i.SlotIndex).First(); // Take last item that is not unique or untradeable

            if (previousGatherItem == null || previousGatherItem.ItemId != GatherItem.ItemId)
            {
                Logging.Write(Colors.Chartreuse, "GatherCollectable: could not find item by slot or name, gathering " + GatherItem.ItemData + " instead.");
            }

            return true;
        }

        private void SetFallbackGatherSpot(Vector3 location, bool useMesh)
        {
            switch (DefaultGatherSpotType)
            {
                // TODO: Smart stealth implementation (where any enemy within x distance and i'm not behind them, use stealth approach and set stealth location as current)
                // If flying, land in area closest to node not in sight of an enemy and stealth.
                case GatherSpotType.StealthApproachGatherSpot:
                case GatherSpotType.StealthGatherSpot:
                    GatherSpot = new StealthGatherSpot { NodeLocation = location, UseMesh = useMesh };
                    break;
                // ReSharper disable once RedundantCaseLabel
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
            if (!gatherRotation.CanBeOverriden || DisableRotationOverride)
            {
                return;
            }

            var rotationAndTypes = Rotations
                .Select(
                    r => new
                             {
                                 Rotation = r.Value,
                                 OverrideValue = r.Value.ResolveOverridePriority(this)
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

        internal async Task CloseGatheringWindow()
        {
            var window = RaptureAtkUnitManager.GetWindowByName("Gathering");

            while (window.IsValid)
            {
                window.SendAction(1, 3, 0xFFFFFFFF);
                await Coroutine.Yield();
            }
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
