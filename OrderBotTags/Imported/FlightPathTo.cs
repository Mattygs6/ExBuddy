namespace ff14bot.NeoProfiles
{
    using ff14bot.Interfaces;
    using Buddy.Coroutines;
    using Clio.Utilities;
    using Clio.XmlEngine;
    using ff14bot.Managers;
    using ff14bot.Helpers;
    using ff14bot.Navigation;
    using System;
    using System.Threading.Tasks;
    using TreeSharp;

    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows.Media;

    using ExBuddy.OrderBotTags;
    using ExBuddy.OrderBotTags.Common;
    using ExBuddy.OrderBotTags.Navigation;

    using ff14bot.Behavior;

    public class FlightPath : IEquatable<FlightPath>
    {
        public Vector3 Start { get; set; }

        public Vector3 End { get; set; }

        // TODO: waypoints

        public bool Equals(FlightPath other)
        {
            return other.Start.Distance3D(this.Start) < 2.7f && other.End.Distance3D(this.End) < 2.7f;
        }
    }

    [XmlElement("FlightPathTo")]
    public class FlightPathTo : FlightVars
    {
        private readonly Stopwatch landingStopwatch = new Stopwatch();

        // TODO make path cache have sliding expiration timeouts
        private static readonly HashSet<FlightPath> Paths = new HashSet<FlightPath>(EqualityComparer<FlightPath>.Default);

        public static FlightPathTo New(IFlightVars flightVars, Vector3 destination, bool dismountAtDestination = false)
        {
            var fp = new FlightPathTo
            {
                Target = destination,
                Radius = flightVars.Radius,
                InverseParabolicMagnitude = flightVars.InverseParabolicMagnitude,
                Smoothing = flightVars.Smoothing,
                MountId = flightVars.MountId,
                ForcedAltitude = flightVars.ForcedAltitude,
                LogWaypoints = flightVars.LogWaypoints,
                ForceLanding = dismountAtDestination || flightVars.ForceLanding
            };
            return fp;
        }

        private bool isDone;
        private readonly IPlayerMover playerMover = new SlideMover();
        private readonly List<FlightPoint> waypoints = new List<FlightPoint>();

        public override bool IsDone { get { return isDone; } }

        [XmlAttribute("XYZ")]
        public Vector3 Target { get; set; }

        [DefaultValue(true)]
        [XmlAttribute("UseStraightPath")]
        public bool UseStraightPath { get; set; }

        protected override Composite CreateBehavior()
        {
            return new ActionRunCoroutine(r => Fly());
        }

        public async Task<bool> Fly()
        {
            waypoints.Clear();
            var from = Core.Player.Location;
            var distance = from.Distance3D(Target);

            if (distance < Radius)
            {
                Logging.Write(Colors.DeepSkyBlue, "FlightPathTo: Already in range -> Me: {0}, Target: {1}", from, Target);
                isDone = true;
                return true;
            }

            await FindWaypoints(Core.Player.Location, Target);

            if (waypoints.Count > 0)
            {
                foreach (var waypoint in waypoints)
                {
                    if (LogWaypoints)
                    {
                        if (waypoint.IsDeviation)
                        {
                            Logging.Write(Colors.DeepSkyBlue, "FlightPathTo: Deviating from course to waypoint: {0}", waypoint.Location);
                        }
                        else
                        {
                            Logging.Write(Colors.DeepSkyBlue, "FlightPathTo: Moving to waypoint: {0}", waypoint.Location);    
                        }
                        
                    }

                    await MoveToWithinRadius(waypoint.Location, Radius);
                }
            }
            else
            {
                Logging.Write(Colors.DeepSkyBlue, "FlightPathTo: No viable path computed for {0}.", Target);
            }

            if (ForceLanding)
            {
                await ForceLand();
            }

            isDone = true;
            return true;
        }

        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static Vector3 Lerp(Vector3 value1, Vector3 value2, float amount)
        {
            return new Vector3(
                Lerp(value1.X, value2.X, amount),
                Lerp(value1.Y, value2.Y, amount),
                Lerp(value1.Z, value2.Z, amount));
        }

        public static float Distance2D(Vector3 l, Vector3 r)
        {
            return Convert.ToSingle(Math.Sqrt((r.X - l.X) * (r.X - l.X) + (r.Z - l.Z) * (r.Z - l.Z)));
        }

        public static Vector3 SampleParabola(Vector3 start, Vector3 end, float height, float t)
        {
            Vector3 direction3 = end - start;
            Vector3 computed = start + t * direction3;
            if (Math.Abs(start.Y - end.Y) < 3.0f)
            {
                //start and end are roughly level, pretend they are - simpler solution with less steps
                computed.Y += (float)(Math.Sin((t * (float)Math.PI))) * height;
            }
            else
            {
                //start and end are not level, gets more complicated
                Vector3 direction2 = end - new Vector3(start.X, end.Y, start.Z);
                Vector3 right = Vector3.Cross(direction3, direction2);
                Vector3 up = Vector3.Cross(right, direction3); // Should this be direction2?
                if (end.Y > start.Y) up = -up;
                up.Normalize();

                computed.Y += ((float)(Math.Sin(t * (float)Math.PI)) * height) * up.Y;
            }

            return computed;
        }

        public async Task FindWaypoints(Vector3 from, Vector3 target)
        {
            var totalDistance = from.Distance3D(target);

            var distance = from.Distance3D(target);
            var desiredNumberOfPoints =
                Math.Max(Math.Floor(distance * Math.Min((1 / Math.Pow(distance, 1.0 / 3.0)) + Smoothing, 1.0)), 1.0);
            desiredNumberOfPoints = Math.Min(desiredNumberOfPoints, Math.Floor(distance));

            // Height will be "Forced height" or no greater than 1 / (the greater of 1 and the inverse parabolic magnitude of the distance and also not more than 100
            var height = Math.Max(ForcedAltitude, Math.Min(distance / Math.Max(1, InverseParabolicMagnitude), 100.0f));

            Vector3 hit;
            Vector3 distances;
            var useStraight = UseStraightPath && !WorldManager.Raycast(from, target, out hit, out distances);

            for (var i = 0.0f + (1.0f / ((float)desiredNumberOfPoints)); i <= 1.0f; i += (1.0f / ((float)desiredNumberOfPoints)))
            {
                Vector3 waypoint;
                if (useStraight)
                {
                    waypoint = StraightPath(from, target, ForcedAltitude, i);
                }
                else
                {
                    waypoint = SampleParabola(from, target, height, i);
                }

                int waypointsRemaining;
                if ((waypointsRemaining = (int)desiredNumberOfPoints - waypoints.Count) > 5)
                {
                    waypoint = waypoint.HeightCorrection();
                }
                else
                {
                    waypoint = waypoint.HeightCorrection(waypointsRemaining);
                }

                waypoints.Add(new FlightPoint { Location = waypoint });

                // Lets give it time to breathe. This helps since creating more than 200 waypoints can take longer than a tick
                // Will continue to monitor.
                if (waypoints.Count % 100 == 0)
                {
                    await Coroutine.Yield();
                }
            }
            
            Logging.Write(Colors.DeepSkyBlue, "FlightPathTo: Created {0} waypoints to fly a distance of {1}", waypoints.Count, totalDistance);
        }

        public Vector3 StraightPath(Vector3 from, Vector3 to, float height, float amount)
        {
            // There probably has to be some small usage of height here, but we don't know how many points there are going to be total.
            var result = Lerp(from, to, amount);

            return result;
        }

        public async Task<bool> EnsureMounted()
        {
            while (!GameObjectManager.LocalPlayer.IsMounted && Behaviors.ShouldContinue)
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
            while (GameObjectManager.LocalPlayer.Location.Distance3D(to) > Radius && Behaviors.ShouldContinue)
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
                    var move = Core.Player.Location.AddRandomDirection2D().GetFloor();
                    await Behaviors.MoveToNoMount(move, false, 0.5f);
                    landingStopwatch.Restart();
                }

                await Coroutine.Yield();
            }

            Logging.WriteDiagnostic(Colors.DeepSkyBlue, "Landing took {0} ms", landingStopwatch.Elapsed);

            landingStopwatch.Reset();

            return true;
        }

        protected override void OnResetCachedDone()
        {
            isDone = false;
        }
    }
}