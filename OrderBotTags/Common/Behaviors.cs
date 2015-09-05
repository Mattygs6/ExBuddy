namespace ExBuddy.OrderBotTags
{
    using System;
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

    public static class Behaviors
    {
        public static async Task<bool> MoveTo(Vector3 destination, bool useMesh, uint mountId, float radius = 2.0f, float navHeight = 5.0f, int inverseParabolicMagnitude = 10, string name = null, bool logFlight = false, bool stopInRange = true, bool dismountAtDestination = false, bool checkIfDestinationIsGround = true)
        {
            var distance3d = Core.Player.Location.Distance3D(destination);

            if (MovementManager.IsFlying || (WorldManager.CanFly && ((distance3d >= CharacterSettings.Instance.MountDistance) || (checkIfDestinationIsGround && !destination.IsGround()))))
            {
                // TODO: need better way to handle initializing and params for this function altogether.
                var fp = new FlightPathTo
                             {
                                 Target = destination,
                                 Radius = radius,
                                 ForcedAltitude = navHeight,
                                 Smoothing = 0.5f,
                                 InverseParabolicMagnitude = inverseParabolicMagnitude,
                                 MountId = (int)mountId,
                                 ForceLanding = dismountAtDestination,
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
                if (!Core.Player.IsMounted && distance3d >= CharacterSettings.Instance.MountDistance && CharacterSettings.Instance.UseMount)
                {
                    // We might need Navigator.Stop();
                    if (mountId > 0)
                    {
                        await CommonTasks.MountUp(mountId);
                    }
                    else
                    {
                        await CommonTasks.MountUp();
                    }
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
            var sprintDistance = Math.Min(20.0f, CharacterSettings.Instance.MountDistance);
            float distance;
            if (useMesh)
            {
                MoveResult moveResult = MoveResult.GeneratingPath;
                while ((distance = Core.Player.Location.Distance3D(destination)) > radius || (!stopInRange && (moveResult != MoveResult.Done || moveResult != MoveResult.ReachedDestination)))
                {
                    if (distance > sprintDistance)
                    {
                        await Sprint();
                    }

                    moveResult = Navigator.NavigationProvider.MoveTo(destination, name);
                    await Coroutine.Yield();
                }

                Navigator.Stop();
            }
            else
            {
                var playerMover = new SlideMover();
 
                while ((distance = Core.Player.Location.Distance3D(destination)) > radius)
                {
                    if (distance > sprintDistance)
                    {
                        await Sprint();
                    }
                    
                    playerMover.MoveTowards(destination);
                    await Coroutine.Yield();
                }

                playerMover.MoveStop();
            }

            return true;
        }

        public static async Task<bool> Sprint()
        {
            if (Actionmanager.IsSprintReady && !Core.Player.IsMounted && Core.Player.CurrentTP == 1000 && MovementManager.IsMoving)
            {
                Actionmanager.Sprint();
            }

            return true;
        }
    }
}