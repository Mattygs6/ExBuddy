namespace ExBuddy.OrderBotTags.Gather
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using Clio.Utilities;
    using Clio.XmlEngine;

    using ff14bot;
    using ff14bot.Enums;
    using ff14bot.Navigation;

    public enum GatherSpotType
    {
        GatherSpot,
        StealthGatherSpot,
        StealthApproachGatherSpot
    }

    public interface IGatherSpot
    {
        Vector3 NodeLocation { get; set; }

        bool IsMatch { get; }

        Task<bool> MoveFromSpot(Func<Task<bool>> unStealthAction);

        Task<bool> MoveToSpot(Func<Task<bool>> stealthAction, Vector3 fallbackLocation, uint mountId, float radius = 2.0f, float navHeight = 5.0f, string name = null, bool logFlight = true);
    }

    [XmlElement("GatherSpot")]
    public class GatherSpot : StealthGatherSpot
    {
        public override Task<bool> MoveFromSpot(Func<Task<bool>> unStealthAction)
        {
            return base.MoveFromSpot(null);
        }

        public override Task<bool> MoveToSpot(
            Func<Task<bool>> stealthAction,
            Vector3 fallbackLocation,
            uint mountId,
            float radius = 2,
            float navHeight = 5,
            string name = null,
            bool logFlight = true)
        {
            return base.MoveToSpot(null, fallbackLocation, mountId, radius, navHeight, name, logFlight);
        }
    }

    [XmlElement("StealthGatherSpot")]
    public class StealthGatherSpot : IGatherSpot
    {
        private Func<bool> conditional;

        [DefaultValue("True")]
        [XmlAttribute("Condition")]
        public string Condition { get; set; }

        [XmlAttribute("NodeLocation")]
        public Vector3 NodeLocation { get; set; }

        [DefaultValue(true)]
        [XmlAttribute("UseMesh")]
        public bool UseMesh { get; set; }

        public bool IsMatch
        {
            get
            {
                if (conditional == null)
                {
                    conditional = ScriptManager.GetCondition(Condition);
                }

                return conditional();
            }
        }

        public virtual async Task<bool> MoveFromSpot(Func<Task<bool>> unStealthAction)
        {
            if (unStealthAction != null)
            {
                await unStealthAction();
            }

            return true;
        }

        public virtual async Task<bool> MoveToSpot(Func<Task<bool>> stealthAction, Vector3 fallbackLocation, uint mountId, float radius = 2.0f, float navHeight = 5.0f, string name = null, bool logFlight = true)
        {
            if (NodeLocation == Vector3.Zero)
            {
                NodeLocation = fallbackLocation;
            }

            var result = await Behaviors.MoveTo(NodeLocation, UseMesh, mountId, radius, navHeight, name, logFlight);

            if (stealthAction != null)
            {
                await stealthAction();
            }

            return result;
        }

        public override string ToString()
        {
            return string.Format("GatherSpot -> NodeLocation: {0}, Condition: {1}, UseMesh: {2}", NodeLocation, Condition, UseMesh);
        }
    }

    [XmlElement("StealthApproachGatherSpot")]
    public class StealthApproachGatherSpot : IGatherSpot
    {
        private Func<bool> conditional;

        [DefaultValue("True")]
        [XmlAttribute("Condition")]
        public string Condition { get; set; }

        [XmlAttribute("NodeLocation")]
        public Vector3 NodeLocation { get; set; }

        [XmlAttribute("StealthLocation")]
        public Vector3 StealthLocation { get; set; }

        [DefaultValue(true)]
        [XmlAttribute("ReturnToStealthLocation")]
        public bool ReturnToStealthLocation { get; set; }

        [DefaultValue(true)]
        [XmlAttribute("UseMesh")]
        public bool UseMesh { get; set; }

        public bool IsMatch
        {
            get
            {
                if (conditional == null)
                {
                    conditional = ScriptManager.GetCondition(Condition);
                }

                return conditional();
            }
        }

        public async Task<bool> MoveFromSpot(Func<Task<bool>> unStealthAction)
        {
            await MoveToLocation(StealthLocation, 1.0f, "Stealth Location");

            if (unStealthAction != null)
            {
                unStealthAction();
            }

            return true;
        }

        public async Task<bool> MoveToSpot(Func<Task<bool>> stealthAction, Vector3 fallbackLocation, uint mountId, float radius = 2.0f, float navHeight = 5.0f, string name = null, bool logFlight = true)
        {
            if (StealthLocation == Vector3.Zero)
            {
                return false;
            }

            if (NodeLocation == Vector3.Zero)
            {
                NodeLocation = fallbackLocation;
            }

            var result = await MoveToStealthLocation(mountId, radius, navHeight, logFlight);

            if (result)
            {
                if (stealthAction != null)
                {
                    await stealthAction();
                }

                result = await MoveToNodeLocation(radius, name);
            }

            return result;
        }

        private async Task<bool> MoveToLocation(Vector3 destination, float radius, string name)
        {
            bool result;
            if (UseMesh)
            {
                result = await Coroutine.Wait(
                    Timeout.Infinite,
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
                    Timeout.Infinite,
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

            return result;
        }

        private async Task<bool> MoveToStealthLocation(uint mountId, float radius, float navHeight, bool logFlight)
        {
            var result = await Behaviors.MoveTo(StealthLocation, UseMesh, mountId, radius, navHeight, "Stealth Location", logFlight);
            return result;
        }

        private async Task<bool> MoveToNodeLocation(float radius, string name)
        {
            return await MoveToLocation(NodeLocation, radius, name);
        }

        public override string ToString()
        {
            return string.Format(
                "StealthApproachGatherSpot -> StealthLocation: {0}, NodeLocation: {1}, Condition: {2}, ReturnToStealthLocation: {3}, UseMesh: {4}",
                StealthLocation,
                NodeLocation,
                Condition,
                ReturnToStealthLocation,
                UseMesh);
        }
    }
}
