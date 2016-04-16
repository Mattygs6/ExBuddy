namespace ExBuddy.Navigation
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Runtime.Caching;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Media;
	using Buddy.Coroutines;
	using Clio.Common;
	using Clio.Utilities;
	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;
	using ExBuddy.Logging;
	using ff14bot;
	using ff14bot.Enums;
	using ff14bot.Interfaces;
	using ff14bot.Managers;
	using ff14bot.Navigation;
	using NeoGaia.ConnectionHandler;

	[LoggerName("FlightNav")]
	public sealed class FlightEnabledNavigator : LogColors, INavigationProvider, IDisposable
	{
		private readonly IFlightNavigationArgs flightNavigationArgs;

		private readonly INavigationProvider innerNavigator;

		private readonly Logger logger;

		private readonly Stopwatch pathGeneratorStopwatch = new Stopwatch();

		private readonly IFlightEnabledPlayerMover playerMover;

		private bool disposed;

		private Vector3 finalDestination;

		private bool generatingPath;

		private Vector3 origin;

		private Vector3 requestedDestination;

		public FlightEnabledNavigator(INavigationProvider innerNavigator)
			: this(innerNavigator, new FlightEnabledSlideMover(Navigator.PlayerMover), new FlightNavigationArgs()) {}

		public FlightEnabledNavigator(
			INavigationProvider innerNavigator,
			IFlightEnabledPlayerMover playerMover,
			IFlightNavigationArgs flightNavigationArgs)
		{
			logger = new Logger(this);
			this.innerNavigator = innerNavigator;
			this.playerMover = playerMover;
			this.flightNavigationArgs = flightNavigationArgs;
			Navigator.NavigationProvider = this;
			CurrentPath = new FlightPath(Vector3.Zero, Vector3.Zero, flightNavigationArgs);

			logger.Verbose(Localization.Localization.FlightEnabledNavigator_Enabled);
		}

		public FlightPath CurrentPath { get; internal set; }

		public override Color Info
		{
			get { return Colors.SkyBlue; }
		}

		public double PathPrecisionSqr
		{
			get { return PathPrecision*PathPrecision; }
		}

		public bool VerboseLogging { get; set; }

		#region IDisposable Members

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				logger.Verbose(Localization.Localization.FlightEnabledNavigator_Dispose);
				Navigator.NavigationProvider = innerNavigator;
				pathGeneratorStopwatch.Stop();
				playerMover.Dispose();
			}
		}

		#endregion

		public static Vector3 Lerp(Vector3 value1, Vector3 value2, float amount)
		{
			return new Vector3(
				MathEx.Lerp(value1.X, value2.X, amount),
				MathEx.Lerp(value1.Y, value2.Y, amount),
				MathEx.Lerp(value1.Z, value2.Z, amount));
		}

		public static explicit operator GaiaNavigator(FlightEnabledNavigator navigator)
		{
			return navigator.innerNavigator as GaiaNavigator;
		}

		public static Vector3 SampleParabola(Vector3 start, Vector3 end, float height, float t)
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

				computed.Y += ((float) (Math.Sin((t*(float) Math.PI)))*height)*up.Y;
			}

			return computed;
		}

		public static Vector3 StraightPath(Vector3 from, Vector3 to, float amount)
		{
			// There probably has to be some small usage of height here, but we don't know how many points there are going to be total.
			var result = Lerp(from, to, amount);

			return result;
		}

		private async Task<GeneratePathResult> GeneratePath(Vector3 start, Vector3 end)
		{
			CurrentPath = new FlightPath(start, end, flightNavigationArgs);

			FlightPath path;
			if (FlightPath.Paths.TryGetValue(CurrentPath.Key, out path))
			{
				CurrentPath = path;

				return GeneratePathResult.SuccessUseExisting;
			}

			if (await CurrentPath.BuildPath())
			{
				if (CurrentPath.Count > 0)
				{
					FlightPath.Paths[CurrentPath.Key] = CurrentPath;

					return GeneratePathResult.Success;
				}
			}

			logger.Error(
				Localization.Localization.FlightEnabledNavigator_NoPath);
			Clear();

			Navigator.NavigationProvider = innerNavigator;
#pragma warning disable 4014
			Task.Factory.StartNew(
#pragma warning restore 4014
				() =>
				{
					Thread.Sleep(10000);
					logger.Info(Localization.Localization.FlightEnabledNavigator_ResetNavigatonProvider);
					Navigator.NavigationProvider = this;
				});

			return GeneratePathResult.Failed;
		}

		private void HandlePathGenerationResult(Task<GeneratePathResult> task)
		{
			switch (task.Result)
			{
				case GeneratePathResult.Success:
					logger.Info(
						Localization.Localization.FlightEnabledNavigator_PathGenerated,
						finalDestination,
						CurrentPath.Count,
						pathGeneratorStopwatch.Elapsed);
					break;
				case GeneratePathResult.SuccessUseExisting:
					logger.Info(
						Localization.Localization.FlightEnabledNavigator_PathFound,
						finalDestination,
						CurrentPath.Count,
						pathGeneratorStopwatch.Elapsed);
					break;
				case GeneratePathResult.Failed:
					logger.Error(Localization.Localization.FlightEnabledNavigator_PathNotViable, finalDestination, origin);
					break;
			}

			pathGeneratorStopwatch.Reset();
			generatingPath = false;
		}

		private MoveResult MoveToNextHop(string name)
		{
			var location = Core.Me.Location;
			// if we are not at the start
			if (CurrentPath.Index > 1)
			{
				Vector3 hit;
				if (WorldManager.Raycast(location, CurrentPath.Current, out hit))
				{
					if (hit.Distance(CurrentPath.End) > flightNavigationArgs.Radius + 1.0f)
					{
						MemoryCache.Default.Add(
							CurrentPath.Current.Location.ToString(),
							CurrentPath.Current,
							DateTimeOffset.Now + TimeSpan.FromSeconds(10));

						logger.Warn(Localization.Localization.FlightEnabledNavigator_CollisionDetected);
						Clear();
						return MoveResult.GeneratingPath;
					}
				}
			}

			double distanceToNextHop = location.Distance3D(CurrentPath.Current);

			if (distanceToNextHop >= PathPrecision)
			{
				//Navigator.PlayerMover.MoveTowards(CurrentPath.Current);
				playerMover.MoveTowards(CurrentPath.Current);
				return MoveResult.Moved;
			}

			if (!CurrentPath.Next())
			{
				logger.Info(Localization.Localization.FlightEnabledNavigator_DestinationReached + location.Distance(finalDestination));
				Clear();

				return MoveResult.ReachedDestination;
			}

			if (!string.IsNullOrWhiteSpace(name))
			{
				name = string.Concat(" (", name, ")");
			}

			logger.Verbose(
				Localization.Localization.FlightEnabledNavigator_HopMoving,
				CurrentPath.Current,
				name,
				location.Distance(CurrentPath.Current));
			if (!ExBuddySettings.Instance.VerboseLogging
			    && (CurrentPath.Index%5 == 0 || CurrentPath.Index == CurrentPath.Count - 1))
			{
				logger.Info(
					Localization.Localization.FlightEnabledNavigator_HopMoving2,
					CurrentPath.Index + 1,
					CurrentPath.Current,
					name,
					location.Distance(CurrentPath.Current));
			}

			//Navigator.PlayerMover.MoveTowards(CurrentPath.Current);
			playerMover.MoveTowards(CurrentPath.Current);
			return MoveResult.Moved;
		}

		private async Task<bool> MoveToNoFlight(Vector3 location)
		{
			var result = MoveResult.GeneratingPath;
			while (Core.Player.Location.Distance3D(location) > PathPrecision || result != MoveResult.ReachedDestination
			       || result != MoveResult.Done)
			{
				generatingPath = true;
				result = innerNavigator.MoveTo(location, "Temporary Waypoint");
				await Coroutine.Yield();
			}

			generatingPath = false;

			return true;
		}

		private bool ShouldGeneratePath(Vector3 target, float radius = 0.0f)
		{
			if (origin != Core.Player.Location)
			{
				if (requestedDestination == target)
				{
					return false;
				}
			}

			if (CurrentPath != null && CurrentPath.Count != 0 && requestedDestination != Vector3.Zero)
			{
				// Find the max diagonal of a cube for given radius, this should be the max distance we could receive from random direction method.
				if (radius > 0 && finalDestination.Distance(target) < Math.Sqrt(3)*radius)
				{
					return false;
				}

				if (requestedDestination.Distance3D(target) <= 2.29)
				{
					return false;
				}
			}

			return true;
		}

		private enum GeneratePathResult
		{
			Failed,

			Success,

			SuccessUseExisting
		}

		#region INavigationProvider Members

		public float PathPrecision
		{
			get { return innerNavigator.PathPrecision; }

			set { innerNavigator.PathPrecision = value; }
		}

		public List<CanFullyNavigateResult> CanFullyNavigateTo(IEnumerable<CanFullyNavigateTarget> targets)
		{
			return CanFullyNavigateToAsync(targets, Core.Player.Location, WorldManager.ZoneId).Result;
		}

		public async Task<List<CanFullyNavigateResult>> CanFullyNavigateToAsync(IEnumerable<CanFullyNavigateTarget> targets)
		{
			return await CanFullyNavigateToAsync(targets, Core.Player.Location, WorldManager.ZoneId);
		}

		public Task<List<CanFullyNavigateResult>> CanFullyNavigateToAsync(
			IEnumerable<CanFullyNavigateTarget> targets,
			Vector3 start,
			ushort zoneid)
		{
			// TODO: not sure how much we will have to mess with this, but when I was using it, it was returning true even for Y values in mid air.
			return innerNavigator.CanFullyNavigateToAsync(targets, start, zoneid);
		}

		public bool CanNavigateFully(Vector3 @from, Vector3 to, float strictDistance)
		{
#pragma warning disable 618
			return innerNavigator.CanNavigateFully(@from, to, strictDistance);
#pragma warning restore 618
		}

		public bool Clear()
		{
			CurrentPath.Reset();

			origin = finalDestination = requestedDestination = Vector3.Zero;
			generatingPath = false;
			pathGeneratorStopwatch.Reset();

			return innerNavigator.Clear();
		}

		public MoveResult MoveTo(Vector3 location, string destination = null)
		{
			if (generatingPath)
			{
				return MoveResult.GeneratingPath;
			}

			if (!playerMover.CanFly || (!MovementManager.IsFlying && !playerMover.ShouldFlyTo(location)))
			{
				return innerNavigator.MoveTo(location, destination);
			}

			var currentLocation = GameObjectManager.LocalPlayer.Location;
			if (location.DistanceSqr(currentLocation) > PathPrecisionSqr)
			{
				if (ShouldGeneratePath(location))
				{
					generatingPath = true;
					origin = currentLocation;
					finalDestination = requestedDestination = location;
					pathGeneratorStopwatch.Restart();
					logger.Info("Generating path on {0} from {1} to {2}", WorldManager.ZoneId, origin, finalDestination);
					GeneratePath(origin, finalDestination).ContinueWith(HandlePathGenerationResult);
					return MoveResult.GeneratingPath;
				}

				if (CurrentPath.Count == 0)
				{
					return MoveResult.ReachedDestination;
				}

				return MoveToNextHop(destination);
			}

			logger.Info("Navigation reached current destination. Within {0}", currentLocation.Distance(location));

			requestedDestination = Vector3.Zero;
			playerMover.MoveStop();
			CurrentPath.Reset();

			return MoveResult.Done;
		}

		public MoveResult MoveToRandomSpotWithin(Vector3 location, float radius, string destination = null)
		{
			if (generatingPath)
			{
				return MoveResult.GeneratingPath;
			}

			if (!playerMover.CanFly || (!MovementManager.IsFlying && !playerMover.ShouldFlyTo(location)))
			{
				return innerNavigator.MoveTo(location, destination);
			}

			var currentLocation = GameObjectManager.LocalPlayer.Location;
			if (location.DistanceSqr(currentLocation) > PathPrecisionSqr)
			{
				if (ShouldGeneratePath(location, radius))
				{
					generatingPath = true;
					origin = currentLocation;
					requestedDestination = location;
					finalDestination = location.AddRandomDirection2D(radius);
					pathGeneratorStopwatch.Restart();
					logger.Info("Generating path on {0} from {1} to {2}", WorldManager.ZoneId, origin, finalDestination);
					GeneratePath(origin, finalDestination).ContinueWith(HandlePathGenerationResult);
					return MoveResult.GeneratingPath;
				}

				if (CurrentPath.Count == 0)
				{
					return MoveResult.ReachedDestination;
				}

				return MoveToNextHop(destination);
			}

			logger.Info("Navigation reached current destination. Within {0}", currentLocation.Distance(location));

			requestedDestination = Vector3.Zero;
			playerMover.MoveStop();
			CurrentPath.Reset();

			return MoveResult.Done;
		}

		#endregion
	}
}