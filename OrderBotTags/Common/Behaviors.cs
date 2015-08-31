namespace ExBuddy.OrderBotTags
{
    using System;
    using System.Threading;
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
        public static async Task<bool> MoveTo(Vector3 destination, bool useMesh, uint mountId, float radius = 2.0f, float navHeight = 5.0f, string name = null, bool logFlight = true, int timeout = Timeout.Infinite, bool stopInRange = true, bool dismountAtDestination = false)
        {
            bool result;
            if (Core.Player.Distance(destination) >= CharacterSettings.Instance.MountDistance && WorldManager.CanFly)
            {
                var fp = new FlightPathTo
                             {
                                 Target = destination,
                                 Radius = Math.Max(radius, 3.0f), // Flying requires a larger radius
                                 NavHeight = navHeight,
                                 Smoothing = 0.5f,
                                 DismountAtDestination = dismountAtDestination,
                                 LogWaypoints = logFlight
                             };

                await fp.Fly();
                result = await Coroutine.Wait(timeout, () => fp.IsDone);
            }
            else
            {
                if (!Core.Player.IsMounted && Core.Player.Distance(destination) >= CharacterSettings.Instance.MountDistance)
                {
                    Navigator.Stop();// Do we need this still?
                    await CommonTasks.MountUp(mountId);

                    await Coroutine.Sleep(1500);// do we need this still
                }

                if (useMesh)
                {
                    result = await Coroutine.Wait(
                        timeout,
                        () =>
                            {
                                Sprint().Wait(timeout);
                                var moveResult = Navigator.NavigationProvider.MoveTo(
                                    destination,
                                    name);

                                if (Core.Player.Location.Distance3D(destination) <= radius)
                                {
                                    Navigator.PlayerMover.MoveStop();
                                    return true;
                                }

                                return moveResult == MoveResult.Done || moveResult == MoveResult.ReachedDestination;
                            });
                }
                else
                {
                    var playerMover = new SlideMover();
                    result = await Coroutine.Wait(
                        timeout,
                        () =>
                            {
                                if (Core.Player.Location.Distance2D(destination) > radius)
                                {
                                    Sprint().Wait(timeout);
                                    playerMover.MoveTowards(destination);
                                    Coroutine.Sleep(200).Wait();
                                    return false;
                                }

                                playerMover.MoveStop();
                                return true;
                            });
                }
                
                if (dismountAtDestination)
                {
                    // TODO: Check if we need sleep still
                    await CommonTasks.StopAndDismount();
                    await Coroutine.Sleep(1000);
                }
            }

            return result;
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