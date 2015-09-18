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

    using ExBuddy.Helpers;
    using ExBuddy.Interfaces;
    using ExBuddy.OrderBotTags.Behaviors;

    using ff14bot;
    using ff14bot.Enums;
    using ff14bot.Helpers;
    using ff14bot.Interfaces;
    using ff14bot.Managers;
    using ff14bot.Navigation;

    using NeoGaia.ConnectionHandler;

    public sealed class FlightEnabledNavigator : INavigationProvider, IDisposable
    {
        private readonly Queue<FlightPoint> queuedFlightPoints = new Queue<FlightPoint>(8); 

        private bool disposed;

        private Vector3 origin;

        private Vector3 finalDestination;

        private Vector3 requestedDestination;

        private readonly Stopwatch pathGeneratorStopwatch = new Stopwatch();

        private bool generatingPath;

        private readonly INavigationProvider innerNavigator;

        private readonly IFlightEnabledPlayerMover playerMover;

        private readonly IFlightNavigationArgs flightNavigationArgs;

        public FlightEnabledNavigator(INavigationProvider innerNavigator)
            : this(innerNavigator, new FlightEnabledSlideMover(Navigator.PlayerMover), new FlightNavigationArgs())
        {
        }

        public FlightEnabledNavigator(INavigationProvider innerNavigator, IFlightEnabledPlayerMover playerMover, IFlightNavigationArgs flightNavigationArgs)
        {
            this.innerNavigator = innerNavigator;
            this.playerMover = playerMover;
            this.flightNavigationArgs = flightNavigationArgs;
            Navigator.NavigationProvider = this;
            Navigator.PlayerMover = playerMover;
            CurrentPath = new FlightPath(Vector3.Zero, Vector3.Zero, flightNavigationArgs);

            Logging.WriteDiagnostic(Colors.DeepSkyBlue, "Replacing Navigator with Flight Navigator.");
        }

        public static explicit operator GaiaNavigator(FlightEnabledNavigator navigator)
        {
            return navigator.innerNavigator as GaiaNavigator;
        }

        public FlightPath CurrentPath { get; internal set; }

        public double PathPrecisionSqr
        {
            get
            {
                return PathPrecision * PathPrecision;
            }
        }

        public bool CanNavigateFully(Vector3 @from, Vector3 to, float strictDistance)
        {
#pragma warning disable 618
            return innerNavigator.CanNavigateFully(@from, to, strictDistance);
#pragma warning restore 618
        }

        public bool Clear()
        {
            CurrentPath.Clear();
            CurrentPath.Index = 0;

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

            Vector3 currentLocation = GameObjectManager.LocalPlayer.Location;
            if (location.DistanceSqr(currentLocation) > PathPrecisionSqr)
            {
                if (ShouldGeneratePath(location))
                {
                    generatingPath = true;
                    origin = currentLocation;
                    finalDestination = requestedDestination = location;
                    pathGeneratorStopwatch.Restart();
                    Logging.WriteDiagnostic(
                        Colors.DeepSkyBlue,
                        "Generating path on {0} from {1} to {2}",
                        WorldManager.ZoneId,
                        origin,
                        finalDestination);
                    GeneratePath(origin, finalDestination).ContinueWith(HandlePathGenerationResult);
                    return MoveResult.GeneratingPath;
                }

                if (CurrentPath.Count == 0)
                {
                    return MoveResult.ReachedDestination;
                }

                return MoveToNextHop(destination);
            }

            Logging.WriteDiagnostic(Colors.DeepSkyBlue, "Navigation reached current destination. Within {0}", currentLocation.Distance(location));

            requestedDestination = Vector3.Zero;
            playerMover.MoveStop();
            CurrentPath.Clear();

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

            Vector3 currentLocation = GameObjectManager.LocalPlayer.Location;
            if (location.DistanceSqr(currentLocation) > PathPrecisionSqr)
            {
                if (ShouldGeneratePath(location, radius))
                {
                    generatingPath = true;
                    origin = currentLocation;
                    requestedDestination = location;
                    finalDestination = location.AddRandomDirection2D(radius);
                    pathGeneratorStopwatch.Restart();
                    Logging.WriteDiagnostic(
                        Colors.DeepSkyBlue,
                        "Generating path on {0} from {1} to {2}",
                        WorldManager.ZoneId,
                        origin,
                        finalDestination);
                    GeneratePath(origin, finalDestination).ContinueWith(HandlePathGenerationResult);
                    return MoveResult.GeneratingPath;
                }

                if (CurrentPath.Count == 0)
                {
                    return MoveResult.ReachedDestination;
                }

                return MoveToNextHop(destination);
            }

            Logging.WriteDiagnostic(Colors.DeepSkyBlue, "Navigation reached current destination. Within {0}", currentLocation.Distance(location));

            requestedDestination = Vector3.Zero;
            playerMover.MoveStop();
            CurrentPath.Clear();

            return MoveResult.Done;
        }

        public List<CanFullyNavigateResult> CanFullyNavigateTo(IEnumerable<CanFullyNavigateTarget> targets)
        {
            return CanFullyNavigateToAsync(targets, Core.Player.Location, WorldManager.ZoneId).Result;
        }

        public async Task<List<CanFullyNavigateResult>> CanFullyNavigateToAsync(
            IEnumerable<CanFullyNavigateTarget> targets)
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

        public float PathPrecision
        {
            get
            {
                return innerNavigator.PathPrecision;
            }

            set
            {
                innerNavigator.PathPrecision = value;
            }
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
                if (radius > 0 && finalDestination.Distance(target) < Math.Sqrt(3) * radius)
                {
                    return false;
                }

                if (requestedDestination.Distance3D(target) <= 2.29999995231628)
                {
                    return false;
                }
            }

            return true;
        }

        enum GeneratePathResult
        {
            Failed,
            Success,
            SuccessUseExisting
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

            Logging.Write(Colors.Red, "Error encountered trying to find a path. Trying innerNavigator for 10 seconds before re-enabling flight.");
            Clear();

            Navigator.NavigationProvider = innerNavigator;
#pragma warning disable 4014
            Task.Factory.StartNew(
#pragma warning restore 4014
                () =>
                {
                    Thread.Sleep(10000);
                    Logging.Write(
                        Colors.DeepSkyBlue,
                        "Resetting NavigationProvider to Flight Navigator.");
                    Navigator.NavigationProvider = this;
                });

            return GeneratePathResult.Failed;
        }

        private void HandlePathGenerationResult(Task<GeneratePathResult> task)
        {
            switch (task.Result)
            {
                case GeneratePathResult.Success:
                    Logging.WriteDiagnostic(
                        Colors.DeepSkyBlue,
                        "Generated path to {0} using {1} hops in {2} ms",
                        finalDestination,
                        CurrentPath.Count,
                        pathGeneratorStopwatch.Elapsed);
                    break;
                case GeneratePathResult.SuccessUseExisting:
                    Logging.WriteDiagnostic(
                        Colors.MediumSpringGreen,
                        "Found existing path to {0} using {1} hops in {2} ms",
                        finalDestination,
                        CurrentPath.Count,
                        pathGeneratorStopwatch.Elapsed);
                    break;
                case GeneratePathResult.Failed:
                    Logging.WriteDiagnostic(
                        Colors.Red,
                        "No viable path found to {0} from {1}",
                        finalDestination,
                        origin);
                    break;
            }

            pathGeneratorStopwatch.Reset();
            generatingPath = false;
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

                computed.Y += ((float)(Math.Sin((t * (float)Math.PI))) * height) * up.Y;
            }

            return computed;
        }

        public static Vector3 StraightPath(Vector3 from, Vector3 to, float amount)
        {
            // There probably has to be some small usage of height here, but we don't know how many points there are going to be total.
            var result = Lerp(from, to, amount);

            return result;
        }

        public static Vector3 Lerp(Vector3 value1, Vector3 value2, float amount)
        {
            return new Vector3(
                MathEx.Lerp(value1.X, value2.X, amount),
                MathEx.Lerp(value1.Y, value2.Y, amount),
                MathEx.Lerp(value1.Z, value2.Z, amount));
        }

        private async Task<bool> MoveToNoFlight(Vector3 location)
        {
            MoveResult result = MoveResult.GeneratingPath;
            while (Core.Player.Location.Distance3D(location) > PathPrecision || result != MoveResult.ReachedDestination || result != MoveResult.Done)
            {
                generatingPath = true;
                result = innerNavigator.MoveTo(location, "Temporary Waypoint");
                await Coroutine.Yield();
            }

            generatingPath = false;

            return true;
        }

        private MoveResult MoveToNextHop(string name)
        {
            Vector3 location = Core.Me.Location;
            Vector3 hit;
            if (WorldManager.Raycast(location, CurrentPath.Current, out hit))
            {
                MemoryCache.Default.Add(
                    CurrentPath.Current.Location.ToString(),
                    CurrentPath.Current,
                    DateTimeOffset.Now + TimeSpan.FromSeconds(10));

                Logging.WriteDiagnostic(Colors.PaleVioletRed, "Collision detected! Generating new path!");
                Clear();
                return MoveResult.GeneratingPath;
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
                Logging.WriteDiagnostic(Colors.DeepSkyBlue,
                    "Navigation reached current destination. Within " + location.Distance(finalDestination));
                Clear();

                return MoveResult.ReachedDestination;
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = string.Concat(" (", name, ")");
            }

            var objArray = new object[]
                               {
                                   "Moving to next hop: ", CurrentPath.Current, name, " D: ",
                                   location.Distance(CurrentPath.Current)
                               };

            Logging.WriteDiagnostic(Colors.DeepSkyBlue, string.Concat(objArray));
            //Navigator.PlayerMover.MoveTowards(CurrentPath.Current);
            playerMover.MoveTowards(CurrentPath.Current);
            return MoveResult.Moved;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Logging.WriteDiagnostic(Colors.DeepSkyBlue, "Putting original Navigator back!");
                Navigator.NavigationProvider = innerNavigator;
                pathGeneratorStopwatch.Stop();
                playerMover.Dispose();
            }
        }
    }
}
