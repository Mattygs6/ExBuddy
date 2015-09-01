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
    using System.ComponentModel;
    using System.Threading.Tasks;
    using TreeSharp;

    using System.Collections.Generic;
    using System.Windows.Media;

    using ff14bot.Behavior;

    [XmlElement("FlightPathTo")]
    public class FlightPathTo : ProfileBehavior
    {
        private bool isDone;
        private readonly IPlayerMover playerMover = new SlideMover();
        private readonly List<Vector3> waypoints = new List<Vector3>();

        public override bool IsDone { get { return isDone; } }

        [XmlAttribute("XYZ")]
        public Vector3 Target { get; set; }

        [XmlAttribute("Radius")]
        public float Radius { get; set; }

        [DefaultValue(0.1f)]
        [XmlAttribute("Smoothing")]
        public float Smoothing { get; set; }

        [DefaultValue(45)]
        [XmlAttribute("MountId")]
        public int MountId { get; set; }

        [DefaultValue(0.0f)]
        [XmlAttribute("NavHeight")]
        public float NavHeight { get; set; }

        [DefaultValue(true)]
        [XmlAttribute("DismountAtDestination")]
        public bool DismountAtDestination { get; set; }

        [XmlAttribute("LogWaypoints")]
        public bool LogWaypoints { get; set; }

        protected override Composite CreateBehavior()
        {
            return new ActionRunCoroutine(r => Fly());
        }

        public async Task<bool> Fly()
        {
            await FindWaypoints(Target);

            if (waypoints.Count > 0)
            {
                for (var i = 0; i < waypoints.Count; i++)
                {
                    if (LogWaypoints)
                    {
                        Logging.Write(Colors.DeepSkyBlue, "FlightPathTo: Moving to waypoint: {0}", waypoints[i]);
                    }

                    var from = i == 0 ? Core.Player.Location : waypoints[i - 1];

                    await MoveToWithinRadius(from, waypoints[i], Radius);
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
                Vector3 travelDirection = end - start;
                Vector3 computed = start + t * travelDirection;
                computed.Y += (float)(Math.Sin((double)(t * (float)Math.PI))) * height;
                return computed;
            }
            else
            {
                //start and end are not level, gets more complicated
                Vector3 travelDirection = end - start;
                Vector3 levelDirecteion = end - new Vector3(start.X, end.Y, start.Z);
                Vector3 right = Vector3.Cross(travelDirection, levelDirecteion);
                Vector3 up = Vector3.Cross(right, travelDirection);
                if (end.Y > start.Y) up = -up;
                Vector3 computed = start + t * travelDirection;
                up.Normalize();

                computed.Y += ((float)(Math.Sin((double)(t * (float)Math.PI))) * height) * up.Y;
                return computed;
            }
        }

        public async Task<List<Vector3>> FindWaypoints(Vector3 target)
        {
            waypoints.Clear();
            var distance = GameObjectManager.LocalPlayer.Location.Distance3D(target);
            var desiredNumberOfPoints = Math.Max(Math.Floor(distance * Math.Min((1/ Math.Pow(distance, 1.0/3.0)) + Smoothing, 1.0)), 1.0);

            // Height will be "Forced height or no greater than 1/5th the distance" and also not more than 100
            var height = Math.Min(Math.Max(NavHeight, distance / 5), 100);
            
            for (var i = 0.0f; i <= 1.0f; i += (1.0f / ((float)desiredNumberOfPoints)))
            {
                var waypoint = await SampleParabola(GameObjectManager.LocalPlayer.Location, target, (float)height, i);
                waypoints.Add(waypoint);
            }

            Logging.Write(Colors.DeepSkyBlue, "FlightPathTo: Created {0} waypoints to fly a distance of {1}", waypoints.Count, distance);

            return waypoints;
        }

        public async Task<bool> PathIsClear(Vector3 from, Vector3 to)
        {
            Vector3 hit, distances;
            var somethingInTheWay = WorldManager.Raycast(from, to, out hit, out distances);

            return !somethingInTheWay;
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

        public async Task<bool> MoveToWithinRadius(Vector3 from, Vector3 to, float radius)
        {
            while (GameObjectManager.LocalPlayer.Location.Distance3D(to) > Radius)
            {
                await EnsureFlying();
                if (!await PathIsClear(from, to))
                {
                    //to.Y += NavHeight;
                }

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