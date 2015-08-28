namespace ExBuddy.OrderBotTags.Gather
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using Clio.XmlEngine;

    using ff14bot;
    using ff14bot.Enums;
    using ff14bot.Helpers;
    using ff14bot.Managers;
    using ff14bot.NeoProfiles;
    using ff14bot.Objects;
    using ff14bot.RemoteWindows;

    using TreeSharp;

    using Action = TreeSharp.Action;

    [XmlElement("GatherCollectable")]
    public class GatherCollectable : ProfileBehavior
    {
        private static readonly Dictionary<ClassJobType, Dictionary<Abilities, uint>> AbilitiesMap =
            new Dictionary<ClassJobType, Dictionary<Abilities, uint>>
                {
                    {
                        ClassJobType.Botanist,
                        new Dictionary<Abilities, uint>
                            {
                                {
                                    Abilities
                                    .Stealth,
                                    212
                                },
                                {
                                    Abilities
                                    .IncreaseGatherChance5,
                                    218
                                },
                                {
                                    Abilities
                                    .Truth,
                                    221
                                },
                                {
                                    Abilities
                                    .CollectorsGlove,
                                    4088
                                },
                                {
                                    Abilities
                                    .MethodicalAppraisal,
                                    4089
                                },
                                {
                                    Abilities
                                    .ImpulsiveAppraisal,
                                    4091
                                },
                                {
                                    Abilities
                                    .DiscerningEye,
                                    4092
                                },
                                {
                                    Abilities
                                    .SingleMind,
                                    4098
                                }
                            }
                    },
                    {
                        ClassJobType.Miner,
                        new Dictionary<Abilities, uint>
                            {
                                {
                                    Abilities
                                    .Stealth,
                                    229
                                },
                                {
                                    Abilities
                                    .IncreaseGatherChance5,
                                    235
                                },
                                {
                                    Abilities
                                    .Truth,
                                    238
                                },
                                {
                                    Abilities
                                    .CollectorsGlove,
                                    4074
                                },
                                {
                                    Abilities
                                    .MethodicalAppraisal,
                                    4075
                                },
                                {
                                    Abilities
                                    .ImpulsiveAppraisal,
                                    4077
                                },
                                {
                                    Abilities
                                    .DiscerningEye,
                                    4078
                                },
                                {
                                    Abilities
                                    .SingleMind,
                                    4084
                                }
                            }
                    }
                };

        private enum Auras : short
        {
            None = -1,

            Stealth = 47,

            TruthOfForests = 221,

            TruthOfMountains = 222,

            DiscerningEye = 757,

            CollectorsGlove = 805
        }

        private enum Abilities : byte
        {
            None,

            Stealth, // = 229,212

            IncreaseGatherChance5, // = 235,218

            Truth, // = 238,221

            CollectorsGlove, // = 4074,4088

            MethodicalAppraisal, // = 4075,4089

            ImpulsiveAppraisal, // = 4077,4091

            DiscerningEye, // = 4078,4092

            SingleMind, // = 4084,4098
        }

        private readonly SpellData cordialSpellData = DataManager.GetItem((uint)CordialType.Cordial).BackingAction;

        private bool isDone;

        private IGatherSpot gatherSpot;

        private GatheringPointObject node;

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
                return Math.Min(Core.Player.MaxGP, WaitForGp);
            }
        }

        [DefaultValue(CordialTime.BeforeGather)]
        [XmlElement("CordialTime")]
        public CordialTime CordialTime { get; set; }

        [DefaultValue(CordialType.Auto)]
        [XmlElement("CordialType")]
        public CordialType CordialType { get; set; }

        [XmlElement("GatherSpots")]
        public List<StealthApproachGatherSpot> GatherSpots { get; set; }

        [DefaultValue(45)]
        [XmlAttribute("MountId")]
        public int MountId { get; set; }

        [DefaultValue(true)]
        [XmlAttribute("LogFlight")]
        public bool LogFlight { get; set; }

        [DefaultValue(5)]
        [XmlAttribute("Slot")]
        public int Slot { get; set; }

        [XmlAttribute("GatherObject")]
        public string GatherObject { get; set; }

        [DefaultValue(3.0f)]
        [XmlAttribute("Distance")]
        public float Distance { get; set; }

        [DefaultValue(2.0f)]
        [XmlAttribute("Radius")]
        public float Radius { get; set; }

        [DefaultValue(600)]
        [XmlAttribute("WaitForGp")]
        public int WaitForGp { get; set; }

        [DefaultValue(5.0f)]
        [XmlAttribute("NavHeight")]
        public float NavHeight { get; set; }

        protected override void OnResetCachedDone()
        {
            isDone = false;
            gatherSpot = null;
            node = null;

        }

        protected override Composite CreateBehavior()
        {
            return
                new PrioritySelector(
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
                        node != null && gatherSpot != null && node.CanGather
                        && node.Location.Distance3D(Core.Player.Location) <= Distance,
                        new Sequence(
                            new ActionRunCoroutine(ctx => BeforeGather()),
                            new ActionRunCoroutine(ctx => Gather()),
                            new ActionRunCoroutine(ctx => AfterGather()))),
                    new Decorator(
                        ret => node != null && gatherSpot != null && !node.CanGather,
                        new ActionRunCoroutine(ctx => MoveFromGatherSpot())));
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
            if (!string.IsNullOrWhiteSpace(GatherObject))
            {
                node =
                    GameObjectManager.GetObjectsOfType<GatheringPointObject>()
                        .FirstOrDefault(
                            gpo =>
                            string.Equals(gpo.EnglishName, GatherObject, StringComparison.InvariantCultureIgnoreCase)
                            && gpo.CanGather);
            }
            else
            {
                node = GameObjectManager.GetObjectsOfType<GatheringPointObject>().FirstOrDefault(gpo => gpo.CanGather);
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
                    () => CastAura(Abilities.Stealth, Auras.Stealth),
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

        private async Task<bool>BeforeGather()
        {
            if (Core.Player.CurrentGP < AdjustedWaitForGp && CordialTime.HasFlag(CordialTime.BeforeGather))
            {
                var eorzeaMinutesTillDespawn = 55 - WorldManager.EorzaTime.Minute;
                var realSecondsTillDespawn = eorzeaMinutesTillDespawn * 35 / 12;
                var realSecondsTillStartGathering = realSecondsTillDespawn - 25;

                if (realSecondsTillStartGathering < cordialSpellData.AdjustedCooldown.Seconds)
                {
                    return true;
                }

                var ticksTillStartGathering = realSecondsTillStartGathering / 3;

                var gp = Core.Player.CurrentGP + ticksTillStartGathering * 5;

                if (gp + 300 > AdjustedWaitForGp)
                {
                    // If we used the cordial or the CordialType is only Cordial, not Auto or HiCordial, then return
                    if (await UseCordial(CordialType.Cordial, realSecondsTillStartGathering) || CordialType == CordialType.Cordial)
                    {
                        return true;
                    }
                }

                // Recalculate: could have no time left at this point
                eorzeaMinutesTillDespawn = 55 - WorldManager.EorzaTime.Minute;
                realSecondsTillDespawn = eorzeaMinutesTillDespawn * 35 / 12;
                realSecondsTillStartGathering = realSecondsTillDespawn - 25;

                if (gp + 400 > AdjustedWaitForGp)
                {
                    if (await UseCordial(CordialType.HiCordial, realSecondsTillStartGathering))
                    {
                        return true;
                    }
                }
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
                    Logging.Write("Using Cordial -> Waiting: " + maxTimeoutSeconds + " CurrentGP: " + Core.Player.CurrentGP);
                    if (await Coroutine.Wait(TimeSpan.FromSeconds(maxTimeoutSeconds), () => cordial.CanUse(Core.Player)))
                    {
                        cordial.UseItem(Core.Player);

                        return true;
                    }
                }
            }

            return false;
        }

        private async Task<bool> Gather()
        {
            await
                CastAura(
                    Abilities.Truth,
                    Core.Player.CurrentJob == ClassJobType.Miner ? Auras.TruthOfMountains : Auras.TruthOfForests);
            await CastAura(Abilities.CollectorsGlove, Auras.CollectorsGlove);

            Poi.Current = new Poi(node, PoiType.Gather);
            Poi.Current.Unit.Interact();

            await Coroutine.Wait(10000, () => GatheringManager.WindowOpen);
            await Coroutine.Sleep(2200);

            var hits = 0;
            GatheringItem item = null;
            while (GatheringManager.WindowOpen && hits < 2)
            {
                await
                    Coroutine.Wait(
                        5000,
                        () => (item = GatheringManager.GetGatheringItemByIndex((uint)this.Slot)) != null);
                if (item != null)
                {
                    item.GatherItem();
                    hits++;
                    await Coroutine.Sleep(2200);
                }
            }

            await GetValidMasterPieceWindow(5000);

            await Cast(Abilities.DiscerningEye);

            await AppraiseAndRebuff();
            await AppraiseAndRebuff();

            await Cast(Abilities.MethodicalAppraisal);

            if (Core.Player.CurrentGP >= 50)
            {
                await Cast(Abilities.IncreaseGatherChance5);
            }

            while (GatheringManager.SwingsRemaining > 0)
            {
                var masterpieceWindow = await GetValidMasterPieceWindow(5000);
                await Coroutine.Wait(5000, () => !SelectYesNoItem.IsOpen);
                while (!SelectYesNoItem.IsOpen)
                {
                    masterpieceWindow.SendAction(1, 1, 0);
                    await Coroutine.Wait(1000, () => SelectYesNoItem.IsOpen);
                }

                ff14bot.RemoteWindows.SelectYesNoItem.Yes();
                await Coroutine.Sleep(2200);
            }

            Poi.Clear("Gather Complete!");

            return true;
        }

        private async Task<bool> CastAura(uint spellId, int auraId = -1)
        {
            bool result;
            if (auraId == -1 || !Core.Player.HasAura(auraId))
            {
                await Coroutine.Wait(3000, () => Actionmanager.CanCast(spellId, Core.Player));
                result = Actionmanager.DoAction(spellId, Core.Player);
            }
            else
            {
                result = false;
            }

            return result;
        }

        private async Task<bool> CastAura(Abilities ability, Auras aura = Auras.None)
        {

            return await CastAura(AbilitiesMap[Core.Player.CurrentJob][ability], (int)aura);
        }

        private async Task<bool> Cast(uint id)
        {
            //Wait till we can cast the spell
            await Coroutine.Wait(5000, () => Actionmanager.CanCast(id, Core.Player));
            var result = Actionmanager.DoAction(id, Core.Player);
            //Wait till we can cast methodical again
            await Coroutine.Wait(5000, () => Actionmanager.CanCast(4075, Core.Player));
            //Wait for aura?
            await Coroutine.Sleep(300);
            return result;
        }

        private async Task<bool> Cast(Abilities ability)
        {
            return await Cast(AbilitiesMap[Core.Player.CurrentJob][ability]);
        }

        private async Task AppraiseAndRebuff()
        {
            await Cast(Abilities.ImpulsiveAppraisal);

            if (Core.Player.HasAura((int)Auras.DiscerningEye))
            {
                await Cast(Abilities.SingleMind);
            }
            else
            {
                await Cast(Abilities.DiscerningEye);
            }
        }

        private async Task<AtkAddonControl> GetValidMasterPieceWindow(int timeoutMs)
        {
            AtkAddonControl atkControl = null;
            await
                Coroutine.Wait(
                    timeoutMs,
                    () =>
                    (atkControl =
                     RaptureAtkUnitManager.Controls.FirstOrDefault(c => c.Name == "GatheringMasterpiece" && c.IsValid))
                    != null);

            return atkControl;
        }
    }
}
