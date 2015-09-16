namespace ExBuddy.OrderBotTags.Common
{
    using System;
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using Clio.Utilities;

    using ExBuddy.OrderBotTags.Navigation;

    using ff14bot;
    using ff14bot.Behavior;
    using ff14bot.Enums;
    using ff14bot.Managers;
    using ff14bot.Navigation;
    using ff14bot.Settings;
    //using ff14bot.NeoProfiles;

    public static class Behaviors
    {
        private static bool shouldContinue;
        static Behaviors()
        {
            ShouldContinue = true;
            TreeRoot.OnStart += bot => ShouldContinue = true;
            TreeRoot.OnStop += bot => ShouldContinue = false;
            //GameEvents.OnPlayerDied += (sender, args) => ShouldContinue = false;
        }

        public static bool ShouldContinue
        {
            get
            {
                return Core.Player.IsAlive && shouldContinue;
            }

            set
            {
                shouldContinue = value;
            }
        }

        public static readonly Func<float, float, bool> DontStopInRange = (d, r) => false;

        public static async Task<bool> MoveTo(Vector3 destination, bool useMesh = true, uint mountId = 0, float radius = 2.0f, string name = null, Func<float, float, bool> stopCallback = null, bool dismountAtDestination = false)
        {
            // ReSharper disable once InconsistentNaming
            var distance3d = Core.Player.Location.Distance3D(destination);

            if (Actionmanager.CanMount == 0 && ((!Core.Player.IsMounted && distance3d >= CharacterSettings.Instance.MountDistance && CharacterSettings.Instance.UseMount) || !destination.IsGround()))
            {
                while (!Core.Player.IsMounted && ShouldContinue)
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

            while (dismountAtDestination && Core.Player.IsMounted && ShouldContinue)
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

                await Coroutine.Yield();
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