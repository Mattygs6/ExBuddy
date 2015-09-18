namespace ExBuddy.Helpers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using Buddy.Coroutines;

    using Clio.Utilities;

    using ExBuddy.Interfaces;
    using ExBuddy.Navigation;
    using ExBuddy.OrderBotTags.Behaviors.Objects;

    using ff14bot;
    using ff14bot.Behavior;
    using ff14bot.Enums;
    using ff14bot.Helpers;
    using ff14bot.Managers;
    using ff14bot.Navigation;
    using ff14bot.Settings;

    public static class Behaviors
    {
        private static bool shouldContinue;
        static Behaviors()
        {
            ShouldContinue = true;
            TreeRoot.OnStart += bot => ShouldContinue = true;
            TreeRoot.OnStop += bot => ShouldContinue = false;
        }

        public static bool ShouldContinue
        {
            get
            {
                return Core.Player.IsAlive && shouldContinue;
            }

            internal set
            {
                shouldContinue = value;
            }
        }

        public static readonly Func<float, float, bool> DontStopInRange = (d, r) => false;

        public static IReturnStrategy GetReturnStrategy()
        {
            var currentZoneId = WorldManager.ZoneId;
            var teleportLocation = WorldManager.AvailableLocations.FirstOrDefault(l => l.ZoneId == currentZoneId);

            if (teleportLocation.AetheryteId == 0)
            {
                return GetReturnStrategyForZoneWithoutAetheryte(currentZoneId);
            }

            return new DefaultReturnStrategy { ZoneId = currentZoneId, AetheryteId = teleportLocation.AetheryteId, InitialLocation = Core.Player.Location };
        }

        public static IReturnStrategy GetReturnStrategyForZoneWithoutAetheryte(ushort zoneId)
        {
            IReturnStrategy strategy;
            switch (zoneId)
            {
                case 399:
                    strategy = new NoAetheryteUseTransportReturnStrategy
                    {
                        InteractDistance = 3.0f,
                        ZoneId = 478,
                        AetheryteId = 75,
                        InitialLocation = Core.Player.Location,
                        NpcId = 1015570,
                        NpcLocation = new Vector3(63.45142f, 207.29f, -2.773367f)
                    };
                    break;
                default:
                    strategy = new NoOpReturnStrategy();
                    break;
            }

            return strategy;
        }

        public static async Task<bool> MoveTo(Vector3 destination, bool useMesh = true, uint mountId = 0, float radius = 2.0f, string name = null, Func<float, float, bool> stopCallback = null, bool dismountAtDestination = false)
        {
            // ReSharper disable once InconsistentNaming
            var distance3d = Core.Player.Location.Distance3D(destination);

            if (Actionmanager.CanMount == 0 && ((!Core.Player.IsMounted && distance3d >= CharacterSettings.Instance.MountDistance && CharacterSettings.Instance.UseMount) || !destination.IsGround()))
            {
                var ticks = 0;
                while (!Core.Player.IsMounted && ticks++ < 10 && ShouldContinue)
                {
                    if (mountId > 0)
                    {
                        if (!await CommonTasks.MountUp(mountId))
                        {
                            if (!await CommonTasks.MountUp(1))
                            {
                                await CommonTasks.MountUp(45);
                            }
                        }
                    }
                    else
                    {
                        await CommonTasks.MountUp();
                    }

                    await Coroutine.Yield();
                }
            }

            await MoveToNoMount(destination, useMesh, radius, name, stopCallback);

            var dismountTicks = 0;
            while (dismountAtDestination && dismountTicks++ < 10 && Core.Player.IsMounted && ShouldContinue)
            {
                if (MovementManager.IsFlying)
                {
                    if (Navigator.PlayerMover is FlightEnabledSlideMover)
                    {
                        Navigator.Stop();
                    }
                    else
                    {
                        MovementManager.StartDescending();
                    }
                }
                else
                {
                    Actionmanager.Dismount();
                }

                await Coroutine.Wait(1000, () => !Core.Player.IsMounted);
            }

            if (dismountTicks > 10)
            {
                Logging.Write(Colors.Red, "Failed to dismount after MoveTo task.");
                return false;
            }

            return true;
        }

        public static async Task<bool> MoveToNoMount(Vector3 destination, bool useMesh = true, float radius = 2.0f, string name = null, Func<float, float, bool> stopCallback = null)
        {
            stopCallback = stopCallback ?? ((d, r) => d <= r);

            var sprintDistance = Math.Min(20.0f, CharacterSettings.Instance.MountDistance);
            float distance;
            if (useMesh)
            {
                var moveResult = MoveResult.GeneratingPath;
                while (ShouldContinue && (!stopCallback(distance = Core.Player.Location.Distance3D(destination), radius) || (stopCallback == DontStopInRange && (moveResult != MoveResult.Done || moveResult != MoveResult.ReachedDestination))))
                {
                    moveResult = Navigator.MoveTo(destination, name);
                    await Coroutine.Yield();

                    if (distance > sprintDistance)
                    {
                        Sprint();
                    }
                }

                Navigator.Stop();
            }
            else
            {
                while (ShouldContinue && !stopCallback(distance = Core.Player.Location.Distance3D(destination), radius))
                {
                    Navigator.PlayerMover.MoveTowards(destination);
                    await Coroutine.Yield();

                    if (distance > sprintDistance)
                    {
                        Sprint();
                    }
                }

                Navigator.PlayerMover.MoveStop();
            }

            return true;
        }

        public static async Task<bool> TeleportTo(ushort zoneId, uint aetheryteId)
        {
            if (WorldManager.ZoneId == zoneId)
            {
                // continue we are in the zone.
                return false;
            }

            var ticks = 0;
            while (MovementManager.IsMoving && ticks++ < 5 && ShouldContinue)
            {
                Navigator.Stop();
                await Coroutine.Sleep(240);
            }

            var casted = false;
            while (WorldManager.ZoneId != zoneId && ShouldContinue)
            {
                if (!Core.Player.IsCasting && casted)
                {
                    break;
                }

                if (!Core.Player.IsCasting && !CommonBehaviors.IsLoading)
                {
                    WorldManager.TeleportById(aetheryteId);
                    await Coroutine.Sleep(500);
                }

                casted = casted || Core.Player.IsCasting;
                await Coroutine.Yield();
            }

            await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading);
            await Coroutine.Wait(5000, () => !CommonBehaviors.IsLoading);

            return true;
        }

        public static async Task<bool> TeleportTo(ushort zoneId)
        {
            var teleportLocation = WorldManager.AvailableLocations.FirstOrDefault(l => l.ZoneId == zoneId);

            if (teleportLocation.AetheryteId == 0)
            {
                return false;
            }

            return await TeleportTo(zoneId, teleportLocation.AetheryteId);
        }

        public static async Task<bool> TeleportTo(uint aetheryteId)
        {
            var zoneId = WorldManager.GetZoneForAetheryteId(aetheryteId);

            if (zoneId == 0)
            {
                return false;
            }

            return await TeleportTo((ushort)zoneId, aetheryteId);
        }

        public static async Task<bool> TeleportTo(LocationData locationData)
        {
            return await TeleportTo(locationData.ZoneId, locationData.AetheryteId);
        }

        public static async Task<bool> TeleportTo(IReturnStrategy returnStrategy)
        {
            return await TeleportTo(returnStrategy.ZoneId, returnStrategy.AetheryteId);
        }

        public static async Task<bool> Unstuck()
        {
            // TODO: do things here for unstuck!
            await CommonTasks.DescendTo(0);
            return true;
        }

        public static bool Sprint()
        {
            if (Actionmanager.IsSprintReady && !Core.Player.IsCasting && !Core.Player.IsMounted && Core.Player.CurrentTP == 1000 && MovementManager.IsMoving)
            {
                Actionmanager.Sprint();
            }

            return true;
        }
    }
}