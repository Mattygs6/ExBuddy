namespace ExBuddy.OrderBotTags.Navigation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using Buddy.Coroutines;

    using Clio.Common;
    using Clio.Utilities;

    using ExBuddy.OrderBotTags.Common;

    using ff14bot.Helpers;
    using ff14bot.Managers;
    using ff14bot.Navigation;

    public class StraightOrParabolicFlightPath : FlightPath
    {
        public StraightOrParabolicFlightPath(Vector3 start, Vector3 end)
            : base(start, end)
        {
        }

        public StraightOrParabolicFlightPath(Vector3 start, Vector3 end, IFlightNavigationArgs flightNavigationArgs)
            : base(start, end, flightNavigationArgs)
        {
        }

        public static Vector3 Arc(Vector3 start, Vector3 end, float height, float t)
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

        protected override async Task<bool> Build()
        {
            var desiredNumberOfPoints =
                Math.Max(Math.Floor(this.Distance * Math.Min((1 / Math.Pow(this.Distance, 1.0 / 3.0)) + this.Smoothing, 1.0)), 1.0);
            desiredNumberOfPoints = Math.Min(desiredNumberOfPoints, Math.Floor(this.Distance));

            // Height will be "Forced height" or no greater than 1 / (the greater of 1 and the inverse parabolic magnitude of the distance and also not more than 100
            var height = Math.Max(this.ForcedAltitude, Math.Min(this.Distance / Math.Max(1, this.InverseParabolicMagnitude), 100.0f));

            Vector3 hit;
            Vector3 distances;
            var useStraight = !WorldManager.Raycast(this.Start, this.End, out hit, out distances);

            for (var i = 0.0f + (1.0f / ((float)desiredNumberOfPoints)); i <= 1.0f; i += (1.0f / ((float)desiredNumberOfPoints)))
            {
                Vector3 waypoint;
                if (useStraight)
                {
                    waypoint = Lerp(this.Start, this.End, i);
                }
                else
                {
                    waypoint = Arc(this.Start, this.End, height, i);
                }

                int waypointsRemaining;
                if ((waypointsRemaining = (int)desiredNumberOfPoints - this.Count) > 5)
                {
                    waypoint = waypoint.HeightCorrection();
                }
                else
                {
                    waypoint = waypoint.HeightCorrection(waypointsRemaining);
                }

                this.Add(new FlightPoint { Location = waypoint });

                // Lets give it time to breathe. This helps since creating more than 200 waypoints can take longer than a tick
                // Will continue to monitor.
                if (this.Count % 100 == 0)
                {
                    await Coroutine.Yield();
                }
            }

            return true;
        }
    }

    public class FlightPath : IndexedList<FlightPoint>, IEquatable<FlightPath>
    {
        public static readonly ConcurrentDictionary<Guid, FlightPath> Paths = new ConcurrentDictionary<Guid, FlightPath>();

        private readonly Vector3 startCenterOfCube;

        private readonly Vector3 endCenterOfCube;

        private readonly ushort zoneId;

        private Guid key;

        private readonly IFlightNavigationArgs flightNavigationArgs;

        private readonly Queue<FlightPoint> queuedFlightPoints = new Queue<FlightPoint>(8);

        public FlightPath(Vector3 start, Vector3 end, ushort zoneId = 0)
            : this(start, end, new FlightNavigationArgs())
        {
        }

        public FlightPath(Vector3 start, Vector3 end, IFlightNavigationArgs flightNavigationArgs, ushort zoneId = 0)
        {
            this.zoneId = zoneId == 0 ? WorldManager.ZoneId : zoneId;

            this.startCenterOfCube = GetCenterOfCube(start, flightNavigationArgs.Radius);
            this.endCenterOfCube = GetCenterOfCube(end, flightNavigationArgs.Radius);
            this.Start = start;
            this.End = end;
            this.flightNavigationArgs = flightNavigationArgs;
        }

        public Vector3 Start { get; private set; }

        public Vector3 End { get; private set; }

        public Guid Key
        {
            get
            {
                if (this.key == Guid.Empty)
                {
                    this.key = string.Concat("S: ", this.startCenterOfCube, ", E: ", this.endCenterOfCube, "Z: ", this.zoneId, ", Args: ", this.flightNavigationArgs).ToGuid();
                }

                return this.key;
            }
        }

        public float Smoothing
        {
            get
            {
                return this.flightNavigationArgs.Smoothing;
            }
        }

        public float ForcedAltitude
        {
            get
            {
                return this.flightNavigationArgs.ForcedAltitude;
            }
        }

        public float InverseParabolicMagnitude
        {
            get
            {
                return this.flightNavigationArgs.InverseParabolicMagnitude;
            }
        }

        public float Distance
        {
            get
            {
                return this.Start.Distance(this.End);
            }
        }

        public float Distance2D
        {
            get
            {
                return this.Start.Distance2D(this.End);
            }
        }

        public static Vector3 GetCenterOfCube(Vector3 vector, float radius)
        {
            var side = radius / (float)(2 * Math.Sqrt(3));

            var x = (vector.X - (vector.X % radius)) + side;
            var y = (vector.Y - (vector.Y % radius)) + side;
            var z = (vector.Z - (vector.Z % radius)) + side;
            var centerPoint = new Vector3(x, y, z);

            return centerPoint;
        }

        public static Guid GetKey(FlightPath flightPath)
        {
            return flightPath.Key;
        }

        public static Guid GetKey(Vector3 start, Vector3 end, IFlightNavigationArgs args)
        {
            return GetKey(new FlightPath(start, end, args));
        }

        public static Vector3 Lerp(Vector3 value1, Vector3 value2, float amount)
        {
            return new Vector3(
                MathEx.Lerp(value1.X, value2.X, amount),
                MathEx.Lerp(value1.Y, value2.Y, amount),
                MathEx.Lerp(value1.Z, value2.Z, amount));
        }

        public async Task<bool> BuildPath()
        {
            Clear();
            Index = 0;
            return await Build();
        }

        protected virtual async Task<bool> Build()
        {
            const int MaxUncorrectedErrors = 20;
            var uncorrectedErrors = 0;
            var from = Start;
            var target = End;
            var distance = from.Distance3D(target);
            var desiredNumberOfPoints =
                Math.Max(Math.Floor(distance * Math.Min((1 / Math.Pow(distance, 1.0 / 2.0)) + flightNavigationArgs.Smoothing, 1.0)), 1.0);
            desiredNumberOfPoints = Math.Min(desiredNumberOfPoints, Math.Floor(distance));

            var distancePerWaypoint = distance / (float)desiredNumberOfPoints;

            var previousWaypoint = from;
            var cleanWaypoints = 0;

            for (var i = 0.0f + (1.0f / ((float)desiredNumberOfPoints)); i <= 1.0f; i += (1.0f / ((float)desiredNumberOfPoints)))
            {
                var waypoint = Lerp(from, target, i);

                // TODO: look into capping distance per waypoint, then also the modifier distance
                var collisions = new Collisions(previousWaypoint, waypoint - previousWaypoint, distancePerWaypoint * 2f);

                Vector3 deviationWaypoint;
                var result = collisions.CollisionResult(queuedFlightPoints.ToArray(), out deviationWaypoint);
                if (result != CollisionFlags.None)
                {
                    // DO THINGS! // check landing + buffer zone of 2.0f
                    if (result.HasFlag(CollisionFlags.Forward)
                        && collisions.PlayerCollider.ForwardHit.Distance3D(target) > flightNavigationArgs.Radius + 1.0f)
                    {
                        if (result.HasFlag(CollisionFlags.Error))
                        {
                            Vector3 hit;
                            Vector3 distances;
                            var alternateCount = 0;
                            // Go in random direction up to the distance of a normal waypoint.
                            var alternateWaypoint = previousWaypoint.AddRandomDirection(distancePerWaypoint);
                            while (WorldManager.Raycast(previousWaypoint, alternateWaypoint, out hit, out distances) && Behaviors.ShouldContinue)
                            {
                                if (alternateCount > 20)
                                {
                                    if (uncorrectedErrors >= MaxUncorrectedErrors)
                                    {
                                        ClearQueuedFlightPoints();
                                        Clear();
                                        Index = 0;
                                        return false;
                                    }

                                    foreach (var fp in queuedFlightPoints)
                                    {
                                        MemoryCache.Default.Add(
                                            fp.Location.ToString(),
                                            fp,
                                            DateTimeOffset.Now + TimeSpan.FromSeconds(30));
                                    }

                                    previousWaypoint = from = this.Last();

                                    desiredNumberOfPoints = desiredNumberOfPoints + queuedFlightPoints.Count;
                                    i = 0.0f + (1.0f / ((float)desiredNumberOfPoints));
                                    cleanWaypoints = 0;

                                    distance = from.Distance3D(target);
                                    distancePerWaypoint = distance / (float)desiredNumberOfPoints;

                                    ClearQueuedFlightPoints();

                                    uncorrectedErrors++;
                                    break;
                                }

                                alternateWaypoint = previousWaypoint.AddRandomDirection(10);
                                alternateCount++;
                            }

                            if (alternateCount > 20)
                            {
                                continue;
                            }

                            deviationWaypoint = alternateWaypoint;
                        }

                        var preHeightCorrect = new FlightPoint { Location = deviationWaypoint, IsDeviation = true };
                        MemoryCache.Default.Add(
                            preHeightCorrect.Location.ToString(),
                            preHeightCorrect,
                            DateTimeOffset.Now + TimeSpan.FromSeconds(10));

                        deviationWaypoint = deviationWaypoint.HeightCorrection(flightNavigationArgs.ForcedAltitude);
                        previousWaypoint = from = deviationWaypoint;

                        var flightPoint = new FlightPoint { Location = deviationWaypoint, IsDeviation = true };
                        MemoryCache.Default.Add(
                            flightPoint.Location.ToString(),
                            flightPoint,
                            DateTimeOffset.Now + TimeSpan.FromSeconds(10));

                        QueueFlightPoint(flightPoint);

                        desiredNumberOfPoints = desiredNumberOfPoints - cleanWaypoints;
                        i = 0.0f + (1.0f / ((float)desiredNumberOfPoints));
                        cleanWaypoints = 0;

                        distance = from.Distance3D(target);
                        distancePerWaypoint = distance / (float)desiredNumberOfPoints;

                        continue;
                    }
                }

                cleanWaypoints++;
                int waypointsRemaining;
                if ((waypointsRemaining = (int)desiredNumberOfPoints - FlightPointCount()) > flightNavigationArgs.ForcedAltitude)
                {
                    waypoint = waypoint.HeightCorrection(flightNavigationArgs.ForcedAltitude);
                }
                else
                {
                    waypoint = waypoint.HeightCorrection(waypointsRemaining);
                }

                previousWaypoint = waypoint;
                QueueFlightPoint(waypoint);
            }

            FlushQueuedFlightPoints();
            return Count > 0;
        }

        public bool Equals(FlightPath other)
        {
            return this.Equals(other.Start, other.End, other.flightNavigationArgs);
        }

        public bool Equals(Vector3 start, Vector3 end, IFlightNavigationArgs args)
        {
            if (Math.Abs(this.flightNavigationArgs.ForcedAltitude - args.ForcedAltitude) > float.Epsilon)
            {
                return false;
            }

            if (Math.Abs(this.flightNavigationArgs.Smoothing - args.Smoothing) > float.Epsilon)
            {
                return false;
            }

            if (Math.Abs(this.flightNavigationArgs.InverseParabolicMagnitude - args.InverseParabolicMagnitude) > float.Epsilon)
            {
                return false;
            }

            return start.Distance3D(this.Start) < args.Radius / 2 && end.Distance3D(this.End) < args.Radius / 2;
        }

        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }

        private int FlightPointCount()
        {
            return Count + queuedFlightPoints.Count;
        }

        private void QueueFlightPoint(FlightPoint flightPoint)
        {
            queuedFlightPoints.Enqueue(flightPoint);

            if (queuedFlightPoints.Count == 8)
            {
                Add(queuedFlightPoints.Dequeue());
            }
        }

        private void ClearQueuedFlightPoints()
        {
            queuedFlightPoints.Clear();
        }

        private void FlushQueuedFlightPoints()
        {
            while (queuedFlightPoints.Count > 0)
            {
                Add(queuedFlightPoints.Dequeue());
            }
        }
    }
}