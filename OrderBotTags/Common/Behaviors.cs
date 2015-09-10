namespace ExBuddy.OrderBotTags
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

    public static class Behaviors
    {
        public static async Task<bool> MoveTo(Vector3 destination, bool useMesh = true, uint mountId = 0, float radius = 2.0f, string name = null, bool stopInRange = true, bool dismountAtDestination = false)
        {
            // ReSharper disable once InconsistentNaming
            var distance3d = Core.Player.Location.Distance3D(destination);

            if ((!Core.Player.IsMounted && distance3d >= CharacterSettings.Instance.MountDistance && CharacterSettings.Instance.UseMount) || !destination.IsGround())
            {
                while (!Core.Player.IsMounted)
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

            await MoveToNoMount(destination, useMesh, radius, name, stopInRange);

            while (dismountAtDestination && Core.Player.IsMounted)
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

        public static async Task<bool> MoveToNoMount(Vector3 destination, bool useMesh = true, float radius = 2.0f, string name = null, bool stopInRange = true)
        {
            var sprintDistance = Math.Min(20.0f, CharacterSettings.Instance.MountDistance);
            float distance;
            if (useMesh)
            {
                var moveResult = MoveResult.GeneratingPath;
                while ((distance = Core.Player.Location.Distance3D(destination)) > radius || (!stopInRange && (moveResult != MoveResult.Done || moveResult != MoveResult.ReachedDestination)))
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
                while ((distance = Core.Player.Location.Distance3D(destination)) > radius)
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

        public static bool Sprint()
        {
            if (Actionmanager.IsSprintReady && !Core.Player.IsCasting &&!Core.Player.IsMounted && Core.Player.CurrentTP == 1000 && MovementManager.IsMoving)
            {
                Actionmanager.Sprint();
            }

            return true;
        }
    }
}