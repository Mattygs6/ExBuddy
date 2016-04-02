
#pragma warning disable 1998

namespace ExBuddy.Navigation
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Caching;
	using System.Threading.Tasks;
	using Buddy.Coroutines;
	using Clio.Common;
	using Clio.Utilities;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ff14bot.Managers;

	public class StraightOrParabolicFlightPath : FlightPath
	{
		public StraightOrParabolicFlightPath(Vector3 start, Vector3 end)
			: base(start, end) {}

		public StraightOrParabolicFlightPath(Vector3 start, Vector3 end, IFlightNavigationArgs flightNavigationArgs)
			: base(start, end, flightNavigationArgs) {}

		protected override async Task<bool> Build()
		{
			var desiredNumberOfPoints =
				Math.Max(Math.Floor(Distance*Math.Min((1/Math.Pow(Distance, 1.0/3.0)) + Smoothing, 1.0)), 1.0);
			desiredNumberOfPoints = Math.Min(desiredNumberOfPoints, Math.Floor(Distance));

			// Height will be "Forced height" or no greater than 1 / (the greater of 1 and the inverse parabolic magnitude of the distance and also not more than 100
			var height = Math.Max(ForcedAltitude, Math.Min(Distance/Math.Max(1, InverseParabolicMagnitude), 100.0f));

			Vector3 hit;
			Vector3 distances;
			var useStraight = !WorldManager.Raycast(Start, End, out hit, out distances);

			for (var i = 0.0f + (1.0f/((float) desiredNumberOfPoints)); i <= 1.0f; i += (1.0f/((float) desiredNumberOfPoints)))
			{
				Vector3 waypoint;
				if (useStraight)
				{
					waypoint = Lerp(Start, End, i);
				}
				else
				{
					waypoint = Arc(Start, End, height, i);
				}

				int waypointsRemaining;
				if ((waypointsRemaining = (int) desiredNumberOfPoints - Count) > 4)
				{
					waypoint = waypoint.HeightCorrection(ForcedAltitude);
				}
				else
				{
					waypoint = waypoint.HeightCorrection(waypointsRemaining);
				}

				Add(new FlightPoint {Location = waypoint});

				// Lets give it time to breathe. This helps since creating more than 200 waypoints can take longer than a tick
				// Will continue to monitor.
				if (Count%100 == 0)
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

		private readonly Vector3 endCenterOfCube;

		private readonly IFlightNavigationArgs flightNavigationArgs;

		private readonly Queue<FlightPoint> queuedFlightPoints = new Queue<FlightPoint>(8);

		private readonly Vector3 startCenterOfCube;

		private readonly ushort zoneId;

		private Guid key;

		public FlightPath(Vector3 start, Vector3 end, ushort zoneId = 0)
			: this(start, end, new FlightNavigationArgs(), zoneId) {}

		public FlightPath(Vector3 start, Vector3 end, IFlightNavigationArgs flightNavigationArgs, ushort zoneId = 0)
		{
			this.zoneId = zoneId == 0 ? WorldManager.ZoneId : zoneId;
			this.flightNavigationArgs = flightNavigationArgs;
			startCenterOfCube = GetCenterOfCube(start, flightNavigationArgs.Radius);
			endCenterOfCube = GetCenterOfCube(end, flightNavigationArgs.Radius);
			Start = start;
			End = end;
		}

		public float Distance
		{
			get { return Start.Distance(End); }
		}

		public float Distance2D
		{
			get { return Start.Distance2D(End); }
		}

		public Vector3 End { get; set; }

		public float ForcedAltitude
		{
			get { return flightNavigationArgs.ForcedAltitude; }
		}

		public float InverseParabolicMagnitude
		{
			get { return flightNavigationArgs.InverseParabolicMagnitude; }
		}

		public Guid Key
		{
			get
			{
				if (key == Guid.Empty)
				{
					key =
						string.Concat("S: ", startCenterOfCube, ", E: ", endCenterOfCube, "Z: ", zoneId, ", Args: ", flightNavigationArgs)
							.ToGuid();
				}

				return key;
			}
		}

		public float Smoothing
		{
			get { return flightNavigationArgs.Smoothing; }
		}

		public Vector3 Start { get; set; }

		#region IEquatable<FlightPath> Members

		public bool Equals(FlightPath other)
		{
			return Equals(other.Start, other.End, other.flightNavigationArgs);
		}

		#endregion

		public static Vector3 Arc(Vector3 start, Vector3 end, float height, float t)
		{
			var direction3 = end - start;
			var computed = start + t*direction3;
			if (Math.Abs(start.Y - end.Y) < 3.0f)
			{
				//start and end are roughly level, pretend they are - simpler solution with less steps
				computed.Y += (float) (Math.Sin((t*(float) Math.PI)))*height;
			}
			else
			{
				//start and end are not level, gets more complicated
				var direction2 = end - new Vector3(start.X, end.Y, start.Z);
				var right = Vector3.Cross(direction3, direction2);
				var up = Vector3.Cross(right, direction3); // Should this be direction2?
				if (end.Y > start.Y)
				{
					up = -up;
				}
				up.Normalize();

				computed.Y += ((float) (Math.Sin(t*(float) Math.PI))*height)*up.Y;
			}

			return computed;
		}

		public async Task<bool> BuildPath()
		{
			Clear();
			Index = 0;
			return await Build();
		}

		public bool Equals(Vector3 start, Vector3 end, IFlightNavigationArgs args)
		{
			if (Math.Abs(flightNavigationArgs.ForcedAltitude - args.ForcedAltitude) > float.Epsilon)
			{
				return false;
			}

			if (Math.Abs(flightNavigationArgs.Smoothing - args.Smoothing) > float.Epsilon)
			{
				return false;
			}

			if (Math.Abs(flightNavigationArgs.InverseParabolicMagnitude - args.InverseParabolicMagnitude) > float.Epsilon)
			{
				return false;
			}

			return start.Distance3D(Start) < args.Radius/2 && end.Distance3D(End) < args.Radius/2;
		}

		public static Vector3 GetCenterOfCube(Vector3 vector, float radius)
		{
			var side = radius/(float) (2*Math.Sqrt(3));

			var x = (vector.X - (vector.X%radius)) + side;
			var y = (vector.Y - (vector.Y%radius)) + side;
			var z = (vector.Z - (vector.Z%radius)) + side;
			var centerPoint = new Vector3(x, y, z);

			return centerPoint;
		}

		public override int GetHashCode()
		{
			return Key.GetHashCode();
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

		public void Reset()
		{
			if (Index != 0)
			{
				Index = 0;
			}
		}

		protected virtual async Task<bool> Build()
		{
			const int MaxUncorrectedErrors = 5;
			var uncorrectedErrors = 0;
			var from = Start;
			var target = End;
			var distance = Distance;
			var desiredNumberOfPoints =
				Math.Max(
					Math.Floor(distance*Math.Min((1/Math.Pow(distance, 1.0/2.0)) + flightNavigationArgs.Smoothing, 1.0)),
					1.0);
			desiredNumberOfPoints = Math.Min(desiredNumberOfPoints, Math.Floor(distance));

			var height = Math.Max(ForcedAltitude, Math.Min(Distance/Math.Max(1, InverseParabolicMagnitude), 100.0f));

			var distancePerWaypoint = distance/(float) desiredNumberOfPoints;

			var previousWaypoint = from;
			var cleanWaypoints = 0;

			Func<Vector3, Vector3, float, float, Vector3> createWaypointFunc = Straight;

			for (var i = 0.0f + (1.0f/((float) desiredNumberOfPoints)); i <= 1.0f; i += (1.0f/((float) desiredNumberOfPoints)))
			{
				Vector3 hit;
				if (cleanWaypoints == 0)
				{
					var useStraight = !WorldManager.Raycast(from, target, out hit);

					if (useStraight)
					{
						createWaypointFunc = Straight;
					}
					else
					{
						createWaypointFunc = Arc;
					}
				}

				var waypoint = createWaypointFunc(@from, target, height, i);

				// TODO: look into capping distance per waypoint, then also the modifier distance
				var collisions = new Collisions(previousWaypoint, waypoint - previousWaypoint, distancePerWaypoint*2f);

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
							var alternateCount = 0;
							// Go in random direction up to the distance of a normal waypoint.
							var alternateWaypoint = previousWaypoint.AddRandomDirection(distancePerWaypoint);
							while (WorldManager.Raycast(previousWaypoint, alternateWaypoint, out hit) && Behaviors.ShouldContinue)
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
										MemoryCache.Default.Add(fp.Location.ToString(), fp, DateTimeOffset.Now + TimeSpan.FromSeconds(30));
									}

									previousWaypoint = from = this.Last();

									desiredNumberOfPoints = desiredNumberOfPoints + queuedFlightPoints.Count;
									i = 0.0f + (1.0f/((float) desiredNumberOfPoints));
									cleanWaypoints = 0;

									distance = from.Distance3D(target);
									distancePerWaypoint = distance/(float) desiredNumberOfPoints;
									height = Math.Max(ForcedAltitude, Math.Min(distance/Math.Max(1, InverseParabolicMagnitude), 100.0f));

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

						var preHeightCorrect = new FlightPoint {Location = deviationWaypoint, IsDeviation = true};
						MemoryCache.Default.Add(
							preHeightCorrect.Location.ToString(),
							preHeightCorrect,
							DateTimeOffset.Now + TimeSpan.FromSeconds(10));

						deviationWaypoint = deviationWaypoint.HeightCorrection(flightNavigationArgs.ForcedAltitude);
						previousWaypoint = from = deviationWaypoint;

						var flightPoint = new FlightPoint {Location = deviationWaypoint, IsDeviation = true};
						MemoryCache.Default.Add(
							flightPoint.Location.ToString(),
							flightPoint,
							DateTimeOffset.Now + TimeSpan.FromSeconds(10));

						QueueFlightPoint(flightPoint);

						desiredNumberOfPoints = desiredNumberOfPoints - cleanWaypoints;
						i = 0.0f + (1.0f/((float) desiredNumberOfPoints));
						cleanWaypoints = 0;

						distance = from.Distance3D(target);
						distancePerWaypoint = distance/(float) desiredNumberOfPoints;
						height = Math.Max(ForcedAltitude, Math.Min(distance/Math.Max(1, InverseParabolicMagnitude), 100.0f));

						continue;
					}
				}

				cleanWaypoints++;
				int waypointsRemaining;
				if ((waypointsRemaining = (int) desiredNumberOfPoints - FlightPointCount()) > flightNavigationArgs.ForcedAltitude)
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

		protected static Vector3 Straight(Vector3 start, Vector3 end, float height, float t)
		{
			return Lerp(start, end, t);
		}

		private void ClearQueuedFlightPoints()
		{
			queuedFlightPoints.Clear();
		}

		private int FlightPointCount()
		{
			return Count + queuedFlightPoints.Count;
		}

		private void FlushQueuedFlightPoints()
		{
			while (queuedFlightPoints.Count > 0)
			{
				Add(queuedFlightPoints.Dequeue());
			}
		}

		private void QueueFlightPoint(FlightPoint flightPoint)
		{
			queuedFlightPoints.Enqueue(flightPoint);

			if (queuedFlightPoints.Count == 8)
			{
				Add(queuedFlightPoints.Dequeue());
			}
		}
	}
}