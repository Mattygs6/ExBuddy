namespace ExBuddy.OrderBotTags.Gather
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using Clio.Utilities;

    using ff14bot;
    using ff14bot.Enums;
    using ff14bot.Navigation;

    public interface IGatherSpot
    {
        Vector3 NodeLocation { get; set; }

        bool IsMatch { get; }

        Task<bool> MoveFromSpot();

        Task<bool> MoveToSpot(Action stealthAction, Vector3 fallbackLocation, uint mountId, float radius = 2.0f, float navHeight = 5.0f, string name = null, bool logFlight = true);
    }

    [Clio.XmlEngine.XmlElement("GatherSpot")]
    public class GatherSpot : IGatherSpot
    {
        private Func<bool> conditional;

        [DefaultValue("True")]
        [Clio.XmlEngine.XmlAttribute("Condition")]
        public string Condition { get; set; }

        [Clio.XmlEngine.XmlAttribute("NodeLocation")]
        public Vector3 NodeLocation { get; set; }

        [DefaultValue(true)]
        [Clio.XmlEngine.XmlAttribute("UseMesh")]
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

        public async Task<bool> MoveFromSpot()
        {
            return true;
        }

        public async Task<bool> MoveToSpot(Action stealthAction, Vector3 fallbackLocation, uint mountId, float radius = 2.0f, float navHeight = 5.0f, string name = null, bool logFlight = true)
        {
            if (NodeLocation == Vector3.Zero)
            {
                NodeLocation = fallbackLocation;
            }

            var result = await Behaviors.MoveTo(NodeLocation, UseMesh, mountId, radius, navHeight, name, logFlight);

            if (stealthAction != null)
            {
                stealthAction();
            }

            return result;
        }
    }

    [Clio.XmlEngine.XmlElement("StealthApproachGatherSpot")]
    public class StealthApproachGatherSpot : IGatherSpot
    {
        private Func<bool> conditional;

        [DefaultValue("True")]
        [Clio.XmlEngine.XmlAttribute("Condition")]
        public string Condition { get; set; }

        [Clio.XmlEngine.XmlAttribute("NodeLocation")]
        public Vector3 NodeLocation { get; set; }

        [Clio.XmlEngine.XmlAttribute("StealthLocation")]
        public Vector3 StealthLocation { get; set; }

        [DefaultValue(true)]
        [Clio.XmlEngine.XmlAttribute("ReturnToStealthLocation")]
        public bool ReturnToStealthLocation { get; set; }

        [DefaultValue(true)]
        [Clio.XmlEngine.XmlAttribute("UseMesh")]
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

        public async Task<bool> MoveFromSpot()
        {
            await MoveToLocation(StealthLocation, 2.0f, "Stealth Location");

            return true;
        }

        public async Task<bool> MoveToSpot(Action stealthAction, Vector3 fallbackLocation, uint mountId, float radius = 2.0f, float navHeight = 5.0f, string name = null, bool logFlight = true)
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
                    stealthAction();
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
    }
}
