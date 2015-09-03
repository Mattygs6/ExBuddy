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
    using System.Windows.Media;

    using ExBuddy.OrderBotTags.Common;

    using ff14bot.Behavior;

    public class FlightPath : IEquatable<FlightPath>
    {
        public Vector3 Start { get; set; }

        public Vector3 End { get; set; }

        // TODO: waypoints

        public bool Equals(FlightPath other)
        {
            return other.Start.Distance3D(this.Start) < 2.0f && other.End.Distance3D(this.End) < 2.0f;
        }
    }

    [XmlElement("FlightPathTo")]
    public class FlightPathTo : FlightVars
    {
        // TODO make path cache have sliding expiration timeouts
        private static readonly HashSet<FlightPath> Paths = new HashSet<FlightPath>(EqualityComparer<FlightPath>.Default);

        public static FlightPathTo New(Vector3 destination)
        {
            var fp = new FlightPathTo { Target = destination };

            // TODO: apply defaults.

            return fp;
        }

        public static FlightPathTo New(FlightVars flightVars, Vector3 destination, bool dismountAtDestination)
        {
            var fp = new FlightPathTo
                         {
                             Target = destination,
                             EdgeDetection = 50.0f,
                             Radius = flightVars.Radius,
                             InverseParabolicMagnitude = 10,
                             Smoothing = 0.1f,
                             MountId = flightVars.MountId,
                             NavHeight = flightVars.NavHeight,
                             LogWaypoints = flightVars.LogWaypoints,
                             DismountAtDestination = dismountAtDestination
                         };
            return fp;
        }

        private bool isDone;
        private readonly IPlayerMover playerMover = new SlideMover();
        private readonly List<Vector3> waypoints = new List<Vector3>();

        public override bool IsDone { get { return isDone; } }

        [XmlAttribute("XYZ")]
        public Vector3 Target { get; set; }

        protected override Composite CreateBehavior()
        {
            return new ActionRunCoroutine(r => Fly());
        }

        public async Task<bool> Fly()
        {
            await FindWaypoints(Target);

            if (waypoints.Count > 0)
            {
                foreach (var waypoint in waypoints)
                {
                    if (LogWaypoints)
                    {
                        Logging.Write(Colors.DeepSkyBlue, "FlightPathTo: Moving to waypoint: {0}", waypoint);
                    }

                    await MoveToWithinRadius(waypoint, Radius);
                }
            }
            else
            {
                Logging.Write(Colors.DeepSkyBlue, "FlightPathTo: No viable path computed for {0}.", Target);
            }

            if (DismountAtDestination)
            {
                await DescendAndDismount();
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

        public static async Task<Vector3> SampleParabola(Vector3 start, Vector3 end, float height, float t)
        {
            if (Math.Abs(start.Y - end.Y) < 3.0f)
            {
                //start and end are roughly level, pretend they are - simpler solution with less steps
                Vector3 direction3 = end - start;
                Vector3 computed = start + t * direction3;
                computed.Y += (float)(Math.Sin((double)(t * (float)Math.PI))) * height;
                return computed;
            }
            else
            {
                //start and end are not level, gets more complicated
                Vector3 direction3 = end - start;
                Vector3 direction2 = end - new Vector3(start.X, end.Y, start.Z);
                Vector3 right = Vector3.Cross(direction3, direction2);
                Vector3 up = Vector3.Cross(right, direction3); // Should this be direction2?
                if (end.Y > start.Y) up = -up;
                Vector3 computed = start + t * direction3;
                up.Normalize();

                computed.Y += ((float)(Math.Sin((double)(t * (float)Math.PI))) * height) * up.Y;
                return computed;
            }
        }

        public async Task FindWaypoints(Vector3 target)
        {
            waypoints.Clear();
            var distance = GameObjectManager.LocalPlayer.Location.Distance3D(target);

            if (distance < Radius)
            {
                Logging.Write(Colors.DeepSkyBlue, "FlightPathTo: Already in range -> Me: {0}, Target{1}", Core.Player.Location, target);
                return;
            }

            var desiredNumberOfPoints = Math.Max(Math.Floor(distance * Math.Min((1 / Math.Pow(distance, 1.0 / 3.0)) + Smoothing, 1.0)), 1.0);

            desiredNumberOfPoints = Math.Min(desiredNumberOfPoints, Math.Floor(distance));


            // Height will be "Forced height" or no greater than 1/5th the distance and also not more than 100
            var height = Math.Max(NavHeight, Math.Min(distance / 5, 100.0f));
            
            for (var i = 0.0f; i <= 1.0f; i += (1.0f / ((float)desiredNumberOfPoints)))
            {
                var waypoint = await SampleParabola(GameObjectManager.LocalPlayer.Location, target, (float)height, i);
                waypoints.Add(waypoint);
            }

            Logging.Write(Colors.DeepSkyBlue, "FlightPathTo: Created {0} waypoints to fly a distance of {1}", waypoints.Count, distance);
        }

        public async Task<bool> PathIsClear(Vector3 to)
        {
            Vector3 hit, distances;



            return true;
        }

        public async Task<bool> TakeFlight()
        {
            return await CommonTasks.TakeOff();
        }

        public async Task<bool> EnsureMounted()
        {
            while (!GameObjectManager.LocalPlayer.IsMounted)
            {
                await CommonTasks.MountUp((uint)MountId);
                await Coroutine.Yield();
            }
            return true;
        }

        public async Task<bool> EnsureFlying()
        {
            await EnsureMounted();
            if (!ff14bot.Managers.MovementManager.IsFlying)
            {
                await TakeFlight();
            }
            return true;
        }

        public async Task<bool> MoveToWithinRadius(Vector3 to, float radius)
        {
            while (GameObjectManager.LocalPlayer.Location.Distance3D(to) > Radius)
            {
                await EnsureFlying();
                ////if (!await PathIsClear(to))
                ////{

                ////}

                playerMover.MoveTowards(to);
                await Coroutine.Yield();
            }
            playerMover.MoveStop();
            return true;
        }

        public async Task<bool> DescendAndDismount()
        {
            while (GameObjectManager.LocalPlayer.IsMounted)
            {
                Actionmanager.Dismount();
                await Coroutine.Yield();
            }

            await Coroutine.Sleep(500);
            return true;
        }

        protected override void OnResetCachedDone()
        {
            isDone = false;
        }
    }
}