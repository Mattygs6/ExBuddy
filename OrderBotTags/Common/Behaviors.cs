namespace ExBuddy.OrderBotTags
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using Clio.Utilities;

    using ff14bot;
    using ff14bot.Behavior;
    using ff14bot.Enums;
    using ff14bot.Managers;
    using ff14bot.Navigation;
    using ff14bot.NeoProfiles;
    using ff14bot.Settings;

    using NeoGaia.ConnectionHandler;

    public static class Behaviors
    {
        private static uint id;
        public static async Task<bool> MoveTo(Vector3 destination, bool useMesh, uint mountId, float radius = 2.0f, float navHeight = 5.0f, string name = null, bool logFlight = false, bool stopInRange = true, bool dismountAtDestination = false)
        {
            if (id == int.MaxValue)
            {
                id = 0;
            }

            var distance3d = Core.Player.Location.Distance3D(destination);
            var target = new CanFullyNavigateTarget { Id = id++, Position = destination };
            var targets = new[] { target };

            var canNav = await Navigator.NavigationProvider.CanFullyNavigateToAsync(targets, Core.Player.Location, WorldManager.ZoneId);
            var canNavResult = canNav.FirstOrDefault(r => r.Id == target.Id);
            if (MovementManager.IsFlying ||
                (WorldManager.CanFly && (distance3d >= CharacterSettings.Instance.MountDistance || (canNavResult != null && (canNavResult.CanNavigate != 1 || canNavResult.PathLength * 0.9 > distance3d)))))
            {
                var fp = new FlightPathTo
                             {
                                 Target = destination,
                                 Radius = Math.Max(radius, 3.0f), // Flying requires a larger radius
                                 NavHeight = navHeight,
                                 MountId = (int)mountId,
                                 Smoothing = 0.5f,
                                 DismountAtDestination = dismountAtDestination,
                                 LogWaypoints = logFlight
                             };

                await fp.Fly();

                while (!fp.IsDone)
                {
                    await Coroutine.Yield();
                }
            }
            else
            {
                if (!Core.Player.IsMounted && distance3d >= CharacterSettings.Instance.MountDistance)
                {
                    // We might need Navigator.Stop();
                    await CommonTasks.MountUp(mountId);
                }

                await MoveToNoMount(destination, useMesh, radius, name, stopInRange);

                if (dismountAtDestination && Core.Player.IsMounted)
                {
                    await CommonTasks.StopAndDismount();
                }
            }

            return true;
        }

        public static async Task<bool> MoveToNoMount(Vector3 destination, bool useMesh, float radius, string name = null, bool stopInRange = true)
        {
            if (useMesh)
            {
                MoveResult moveResult = MoveResult.GeneratingPath;
                while (Core.Player.Location.Distance2D(destination) > radius || (!stopInRange && (moveResult != MoveResult.Done || moveResult != MoveResult.ReachedDestination)))
                {
                    await Sprint();
                    moveResult = Navigator.NavigationProvider.MoveTo(destination, name);
                    await Coroutine.Yield();
                }

                Navigator.Stop();
            }
            else
            {
                var playerMover = new SlideMover();

                while (Core.Player.Location.Distance2D(destination) > radius)
                {
                    await Sprint();
                    playerMover.MoveTowards(destination);
                    await Coroutine.Yield();
                }

                playerMover.MoveStop();
            }

            return true;
        }

        public static async Task<bool> Sprint()
        {
            if (!Core.Player.IsMounted && Core.Player.CurrentTP == 1000 && MovementManager.IsMoving
                && Actionmanager.IsSprintReady)
            {
                Actionmanager.Sprint();
            }

            return true;
        }
    }
}