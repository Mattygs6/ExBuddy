namespace ExBuddy.OrderBotTags.Behaviors
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using Buddy.Coroutines;

    using Clio.Utilities;
    using Clio.XmlEngine;

    using ExBuddy.Helpers;
    using ExBuddy.Navigation;

    using ff14bot.Behavior;
    using ff14bot.Interfaces;
    using ff14bot.Managers;
    using ff14bot.Navigation;

    using TreeSharp;

    [XmlElement("FlightPathTo")]
    public class FlightPathTo : FlightVars
    {
        private readonly Stopwatch landingStopwatch = new Stopwatch();

        private bool isDone;
        private readonly IPlayerMover playerMover = new SlideMover();

        public override bool IsDone { get { return isDone; } }

        protected override Color Info
        {
            get
            {
                return Colors.DeepSkyBlue;
            }
        }

        [XmlAttribute("XYZ")]
        public Vector3 Target { get; set; }

        protected override Composite CreateBehavior()
        {
            return new ActionRunCoroutine(r => Fly());
        }

        public async Task<bool> Fly()
        {
            FlightPath flightPath = new StraightOrParabolicFlightPath(Me.Location, Target, this);

            var distance = flightPath.Distance;

            if (distance < Radius)
            {
                Logger.Info("Already in range -> Start: {0}, End: {1}", flightPath.Start, flightPath.End);
                isDone = true;
                return true;
            }

            FlightPath path;
            if (FlightPath.Paths.TryGetValue(flightPath.Key, out path))
            {
                flightPath = path;
                Logger.Info(
                    "Using existing FlightPath {0} from {1} to {2}",
                    flightPath.Key,
                    flightPath.Start,
                    flightPath.End);
            }
            else
            {
                Logger.Info(
                    "Building new FlightPath {0} from {1} to {2}",
                    flightPath.Key,
                    flightPath.Start,
                    flightPath.End);

                if (await flightPath.BuildPath())
                {
                    FlightPath.Paths[flightPath.Key] = flightPath;
                }
            }

            if (flightPath.Count > 0)
            {
                do
                {
                    if (LogWaypoints)
                    {
                        if (flightPath.Current.IsDeviation)
                        {
                            Logger.Info("Deviating from course to waypoint: {0}", flightPath.Current);
                        }
                        else
                        {
                            Logger.Info("Moving to waypoint: {0}", flightPath.Current);
                        }

                    }

                    await MoveToWithinRadius(flightPath.Current, Radius);
                }
                while (flightPath.Next());

                flightPath.Reset();
            }
            else
            {
                Logger.Info("No viable path computed for {0}.", Target);
            }

            if (ForceLanding)
            {
                await ForceLand();
            }

            isDone = true;
            return true;
        }

        public async Task<bool> EnsureMounted()
        {
            while (!Me.IsMounted && Behaviors.ShouldContinue)
            {
                if (MountId > 0)
                {
                    await CommonTasks.MountUp((uint)MountId);
                }
                else
                {
                    await CommonTasks.MountUp();
                }

                await Coroutine.Yield();
            }
            return true;
        }

        public async Task<bool> EnsureFlying()
        {
            await EnsureMounted();
            if (!ff14bot.Managers.MovementManager.IsFlying)
            {
                return await CommonTasks.TakeOff();
            }
            return true;
        }

        public async Task<bool> MoveToWithinRadius(Vector3 to, float radius)
        {
            while (Me.Location.Distance3D(to) > Radius && Behaviors.ShouldContinue)
            {
                await EnsureFlying();

                playerMover.MoveTowards(to);
                await Coroutine.Yield();
            }
            playerMover.MoveStop();
            return true;
        }

        public async Task<bool> ForceLand()
        {
            landingStopwatch.Restart();
            while (MovementManager.IsFlying && Behaviors.ShouldContinue)
            {
                MovementManager.StartDescending();

                if (landingStopwatch.ElapsedMilliseconds > 2000 && MovementManager.IsFlying)
                {
                    var move = Me.Location.AddRandomDirection2D().GetFloor();
                    await Behaviors.MoveToNoMount(move, false, 0.5f);
                    landingStopwatch.Restart();
                }

                await Coroutine.Yield();
            }

            Logger.Info("Landing took {0} ms", landingStopwatch.Elapsed);

            landingStopwatch.Reset();

            return true;
        }

        protected override void OnResetCachedDone()
        {
            isDone = false;
        }
    }
}