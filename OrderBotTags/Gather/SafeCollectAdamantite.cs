namespace ExBuddy.OrderBotTags.Gather
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using Clio.Utilities;
    using Clio.XmlEngine;

    using ff14bot;
    using ff14bot.Behavior;
    using ff14bot.Enums;
    using ff14bot.Helpers;
    using ff14bot.Managers;
    using ff14bot.Navigation;
    using ff14bot.NeoProfiles;
    using ff14bot.Objects;

    using TreeSharp;

    [XmlElement("SafeCollectAdamantite")]
    public class SafeCollectAdamantite : ProfileBehavior
    {
        enum Abilities : ushort
        {
            Stealth = 229,
            SharpVision = 235,
            TruthOfMountains = 238,
            CollectorsGlove = 4074,
            MethodicalAppraisal = 4075,
            ImpulsiveAppraisal = 4077,
            DiscerningEye = 4078,
            SingleMind = 4084
        }

        private Vector3 initialLocation;
        private bool isDone;

        public override bool IsDone
        {
            get
            {
                return isDone;
            }
        }

        [DefaultValue(-1)]
        [XmlAttribute("TeleportId")]
        public int TeleportId { get; set; }

        protected override void OnStart()
        {
            this.initialLocation = Core.Player.Location;
        }

        protected override void OnResetCachedDone()
        {
            isDone = false;
        }

        protected override Composite CreateBehavior()
        {
            return new ActionRunCoroutine(ctx => Gather());
        }

        private async Task<bool> Gather()
        {
            var node =
                GameObjectManager.GetObjectsOfType<GatheringPointObject>()
                    .FirstOrDefault(
                        gpo =>
                        gpo.EnglishName == "Unspoiled Mineral Deposit"
                        && gpo.IsVisible && gpo.IsValid);

            if (node == null)
            {
                return false;
            }

            MovementManager.SetFacing2D(node.Location);
            var safeSpot = GetSafeSpot(Core.Player.Heading);

            var fp = new FlightPathTo { Target = safeSpot.Item1, Radius = 3.0f, NavHeight = 10.0f, MountId = 45, Smoothing = 0.2f, DismountAtDestination = true, LogWaypoints = true };
            fp.Start();
            await fp.Fly();
            await Coroutine.Wait(Timeout.Infinite, () => fp.IsDone);

            //Stealth
            await Coroutine.Wait(3000, () => Actionmanager.CanCast((uint)Abilities.Stealth, Core.Player));
            Actionmanager.DoAction((uint)Abilities.Stealth, Core.Player);

            //Truth of Mountains
            if (!Core.Player.HasAura(222))
            {
                await Coroutine.Wait(3000, () => Actionmanager.CanCast((uint)Abilities.TruthOfMountains, Core.Player));
                Actionmanager.DoAction((uint)Abilities.TruthOfMountains, Core.Player);
            }

            //Collector's Glove
            if (!Core.Player.HasAura(805))
            {
                await Coroutine.Wait(3000, () => Actionmanager.CanCast((uint)Abilities.CollectorsGlove, Core.Player));
                Actionmanager.DoAction((uint)Abilities.CollectorsGlove, Core.Player);
            }

            //Move to node
            await Coroutine.Wait(
                20000,
                () =>
                    {
                        var result = Navigator.NavigationProvider.MoveTo(safeSpot.Item2);

                        return result == MoveResult.Done || result == MoveResult.ReachedDestination;
                    });

            Poi.Current = new Poi(node, PoiType.Gather);
            Poi.Current.Unit.Interact();

            await Coroutine.Wait(5000, () => GatheringManager.WindowOpen);

            await Coroutine.Sleep(1000);
            GatheringManager.GetGatheringItemByIndex(5).GatherItem();

            await Coroutine.Sleep(4000);
            GatheringManager.GetGatheringItemByIndex(5).GatherItem();

            await Coroutine.Sleep(500);
            RaptureAtkUnitManager.Update();
            await Coroutine.Wait(5000, () => RaptureAtkUnitManager.GetWindowByName("GatheringMasterpiece") != null);

            await Cast(Abilities.DiscerningEye);
            await Cast(Abilities.ImpulsiveAppraisal);

            if (Core.Player.HasAura(757))
            {
                await Cast(Abilities.SingleMind);
            }
            else
            {
                await Cast(Abilities.DiscerningEye);
            }

            await Cast(Abilities.ImpulsiveAppraisal);

            if (Core.Player.HasAura(757))
            {
                await Cast(Abilities.SingleMind);
            }
            else
            {
                await Cast(Abilities.DiscerningEye);
            }

            await Cast(Abilities.MethodicalAppraisal);

            if (Core.Player.CurrentGP >= 50)
            {
                await Cast(Abilities.SharpVision);
            }

            while (GatheringManager.SwingsRemaining > 0)
            {
                await Coroutine.Sleep(500);

                RaptureAtkUnitManager.Update();

                await Coroutine.Wait(5000, () => RaptureAtkUnitManager.GetWindowByName("GatheringMasterpiece") != null);
                while (!ff14bot.RemoteWindows.SelectYesNoItem.IsOpen)
                {
                    RaptureAtkUnitManager.GetWindowByName("GatheringMasterpiece").SendAction(1, 1, 0);
                    await Coroutine.Wait(1000, () => ff14bot.RemoteWindows.SelectYesNoItem.IsOpen);
                }

                ff14bot.RemoteWindows.SelectYesNoItem.Yes();
                await Coroutine.Wait(5000, () => !ff14bot.RemoteWindows.SelectYesNoItem.IsOpen);

                await Coroutine.Sleep(500);
            }

            Poi.Clear("Gather Complete!");

            if (TeleportId == -1)
            {
                await Coroutine.Wait(
                    20000,
                    () =>
                    {
                        var result = Navigator.NavigationProvider.MoveTo(safeSpot.Item1);

                        return result == MoveResult.Done || result == MoveResult.ReachedDestination;
                    });

                fp = new FlightPathTo { Target = initialLocation, Radius = 3.0f, NavHeight = 25.0f, MountId = 45, Smoothing = 0.2f, DismountAtDestination = false, LogWaypoints = true };
                fp.Start();
                await fp.Fly();
                await Coroutine.Wait(Timeout.Infinite, () => fp.IsDone);

                isDone = true;
                return true;
            }

            WorldManager.TeleportById((uint)TeleportId);
            isDone = true;
            return true;
        }

        private Tuple<Vector3, Vector3> GetSafeSpot(float heading)
        {
            if (heading >= 5.7 && heading < 1.5)
            {
                return new Tuple<Vector3, Vector3>(new Vector3(84.4492f, -1.340709f, -790.6899f), new Vector3(79.85004f, -0.2340581f, -805.6276f));
            }
            if (heading >= 1.5 && heading < 3.6)
            {
                return new Tuple<Vector3, Vector3>(new Vector3(64.96072f, -2.004024f, -870.546571f), new Vector3(71.80842f, -0.29418f, -856.2421f));
            }

            return new Tuple<Vector3, Vector3>(new Vector3(20.19515f, -2.000566f, -821.9562f), new Vector3(24.25113f, -0.01400313f, -832.7057f));
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

        private async Task<bool> Cast(Abilities id)
        {
            return await Cast((uint)id);
        }
    }
}
