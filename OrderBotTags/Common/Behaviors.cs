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
    using ff14bot.Settings;

    public static class Behaviors
    {
        public static async Task<bool> MoveTo(Vector3 destination, bool useMesh, uint mountId, float radius = 2.0f, string name = null, bool stopInRange = true, bool dismountAtDestination = false)
        {
            // ReSharper disable once InconsistentNaming
            var distance3d = Core.Player.Location.Distance3D(destination);

            if (!Core.Player.IsMounted && distance3d >= CharacterSettings.Instance.MountDistance && CharacterSettings.Instance.UseMount)
            {
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

            while (dismountAtDestination && Core.Player.IsMounted)
            {
                if (MovementManager.IsFlying)
                {
                    Navigator.Stop();
                }
                else
                {
                    Actionmanager.Dismount();
                }

                await Coroutine.Yield();
            }

            return true;
        }

        public static async Task<bool> MoveToNoMount(Vector3 destination, bool useMesh, float radius, string name = null, bool stopInRange = true)
        {
            var sprintDistance = Math.Min(20.0f, CharacterSettings.Instance.MountDistance);
            float distance;
            if (useMesh)
            {
                var moveResult = MoveResult.GeneratingPath;
                while ((distance = Core.Player.Location.Distance3D(destination)) > radius || (!stopInRange && (moveResult != MoveResult.Done || moveResult != MoveResult.ReachedDestination)))
                {
                    if (distance > sprintDistance)
                    {
                        Sprint();
                    }

                    moveResult = Navigator.MoveTo(destination, name);
                    await Coroutine.Yield();
                }

                Navigator.Stop();
            }
            else
            {
                while ((distance = Core.Player.Location.Distance3D(destination)) > radius)
                {
                    if (distance > sprintDistance)
                    {
                        Sprint();
                    }

                    Navigator.PlayerMover.MoveTowards(destination);
                    await Coroutine.Yield();
                }

                Navigator.PlayerMover.MoveStop();
            }

            return true;
        }

        public static bool Sprint()
        {
            if (Actionmanager.IsSprintReady && !Core.Player.IsMounted && Core.Player.CurrentTP == 1000 && MovementManager.IsMoving)
            {
                Actionmanager.Sprint();
            }

            return true;
        }
    }
}