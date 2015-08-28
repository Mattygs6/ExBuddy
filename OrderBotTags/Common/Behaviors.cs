namespace ExBuddy.OrderBotTags
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using Clio.Utilities;

    using ff14bot;
    using ff14bot.Enums;
    using ff14bot.Managers;
    using ff14bot.Navigation;
    using ff14bot.NeoProfiles;
    using ff14bot.Settings;

    public static class Behaviors
    {
        public static async Task<bool> MoveTo(Vector3 destination, bool useMesh, uint mountId, float radius = 2.0f, float navHeight = 5.0f, string name = null, bool logFlight = true, int timeout = Timeout.Infinite)
        {
            bool result;
            if (!Core.Player.IsMounted && Core.Player.Distance(destination) >= CharacterSettings.Instance.MountDistance)
            {
                Navigator.Stop();
                Actionmanager.Mount(mountId);
                await Coroutine.Sleep(1500);
            }

            if (Core.Player.IsMounted && WorldManager.CanFly)
            {
                var fp = new FlightPathTo
                             {
                                 Target = destination,
                                 Radius = Math.Max(radius, 3.0f), // Flying requires a larger radius
                                 NavHeight = navHeight,
                                 Smoothing = 0.2f,
                                 DismountAtDestination = true,
                                 LogWaypoints = logFlight
                             };
                fp.Start();
                await fp.Fly();
                result = await Coroutine.Wait(timeout, () => fp.IsDone);
            }
            else
            {
                if (useMesh)
                {
                    result = await Coroutine.Wait(
                        timeout,
                        () =>
                            {
                                var moveResult = Navigator.NavigationProvider.MoveTo(
                                    destination,
                                    name);

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
                                    playerMover.MoveTowards(destination);
                                    Coroutine.Sleep(200).Wait();
                                    return false;
                                }

                                playerMover.MoveStop();
                                return true;
                            });
                }

                Actionmanager.Dismount();
                await Coroutine.Sleep(1000);
            }

            return result;
        }
    }
}