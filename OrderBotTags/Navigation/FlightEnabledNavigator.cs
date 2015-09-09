namespace ExBuddy.OrderBotTags.Navigation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using Buddy.Coroutines;

    using Clio.Common;
    using Clio.Utilities;

    using ExBuddy.OrderBotTags.Common;

    using ff14bot;
    using ff14bot.Enums;
    using ff14bot.Helpers;
    using ff14bot.Interfaces;
    using ff14bot.Managers;
    using ff14bot.Navigation;

    using NeoGaia.ConnectionHandler;

    public sealed class FlightEnabledNavigator : INavigationProvider, IDisposable
    {
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
            CurrentPath = new IndexedList<FlightPoint>(32);

            Logging.WriteDiagnostic(Colors.DeepSkyBlue, "Replacing Navigator with Flight Navigator.");
        }

        public static explicit operator GaiaNavigator(FlightEnabledNavigator navigator)
        {
            return navigator.innerNavigator as GaiaNavigator;
        }

        public IndexedList<FlightPoint> CurrentPath { get; protected internal set; }

        public double PathPrecisionSqr
        {
            get
            {
                return this.PathPrecision * this.PathPrecision;
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
            if (location.DistanceSqr(currentLocation) > this.PathPrecisionSqr)
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

            this.requestedDestination = Vector3.Zero;
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
            if (location.DistanceSqr(currentLocation) > this.PathPrecisionSqr)
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

            this.requestedDestination = Vector3.Zero;
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
            if (this.origin != Core.Player.Location)
            {
                if (this.requestedDestination == target)
                {
                    return false;
                }
            }

            if (CurrentPath != null && CurrentPath.Count != 0 && this.requestedDestination != Vector3.Zero)
            {
                // Find the max diagonal of a cube for given radius, this should be the max distance we could receive from random direction method.
                if (radius > 0 && this.finalDestination.Distance(target) < Math.Sqrt(3) * radius)
                {
                    return false;
                }

                if (this.requestedDestination.Distance3D(target) <= 2.29999995231628)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> GeneratePath(Vector3 start, Vector3 end)
        {
            CurrentPath.Clear();
            CurrentPath.Index = 0;

            return await FindWaypoints(start, end);
        }

        private void HandlePathGenerationResult(Task<bool> task)
        {
            if (task.Result)
            {
                Logging.WriteDiagnostic(
                    Colors.DeepSkyBlue,
                    "Generated path to {0} using {1} hops in {2} ms",
                    finalDestination,
                    CurrentPath.Count,
                    pathGeneratorStopwatch.Elapsed);
            }
            else
            {
                Logging.WriteDiagnostic(
                    Colors.Red,
                    "No viable path found to {0} from {1}",
                    finalDestination,
                    origin);
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


#pragma warning disable 1998
        private async Task<bool> FindWaypoints(Vector3 from, Vector3 target)
#pragma warning restore 1998
        {
            //var totalDistance = from.Distance3D(target);

            var distance = from.Distance3D(target);
            var desiredNumberOfPoints =
                Math.Max(Math.Floor(distance * Math.Min((1 / Math.Pow(distance, 1.0 / 2.0)) + flightNavigationArgs.Smoothing, 1.0)), 1.0);
            desiredNumberOfPoints = Math.Min(desiredNumberOfPoints, Math.Floor(distance));

            var distancePerWaypoint = distance / (float)desiredNumberOfPoints;

            Vector3 hit;
            Vector3 distances;
            var previousWaypoint = from;
            var cleanWaypoints = 0;

            for (var i = 0.0f + (1.0f / ((float)desiredNumberOfPoints)); i <= 1.0f; i += (1.0f / ((float)desiredNumberOfPoints)))
            {

                var waypoint = StraightPath(@from, target, i);

                // TODO: look into capping distance per waypoint, then also the modifier distance
                var collisions = new Collisions(previousWaypoint, waypoint - previousWaypoint, distancePerWaypoint *  2f);

                Vector3 deviationWaypoint;
                var result = collisions.CollisionResult(out deviationWaypoint);
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
                            var alternateWaypoint = previousWaypoint.AddRandomDirection(distancePerWaypoint / (float)Math.Sqrt(3));
                            while (WorldManager.Raycast(previousWaypoint, alternateWaypoint, out hit, out distances))
                            {
                                if (alternateCount > 20)
                                {
                                    Logging.Write(Colors.Red, "Error encountered trying to find a path. Trying innerNavigator for 10 seconds before re-enabling flight.");
                                    this.Clear();
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
                                    return false;
                                }
                                alternateWaypoint = previousWaypoint.AddRandomDirection(10);
                                alternateCount++;
                            }

                            deviationWaypoint = alternateWaypoint;
                        }

                        deviationWaypoint = deviationWaypoint.HeightCorrection(flightNavigationArgs.ForcedAltitude);
                        previousWaypoint = from = deviationWaypoint;
                        CurrentPath.Add(new FlightPoint { Location = deviationWaypoint, IsDeviation = true });


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
                if ((waypointsRemaining = (int)desiredNumberOfPoints - CurrentPath.Count) > flightNavigationArgs.ForcedAltitude)
                {
                    waypoint = waypoint.HeightCorrection(flightNavigationArgs.ForcedAltitude);
                }
                else
                {
                    waypoint = waypoint.HeightCorrection(waypointsRemaining);
                }

                previousWaypoint = waypoint;
                CurrentPath.Add(new FlightPoint { Location = waypoint });
            }


            return true;
        }

        private async Task<bool> MoveToNoFlight(Vector3 location)
        {
            MoveResult result = MoveResult.GeneratingPath;
            while (Core.Player.Location.Distance3D(location) > PathPrecision || result != MoveResult.ReachedDestination || result != MoveResult.Done)
            {
                generatingPath = true;
                result = this.innerNavigator.MoveTo(location, "Temporary Waypoint");
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
                Logging.WriteDiagnostic(Colors.PaleVioletRed, "Collision detected! Generating new path!");
                this.Clear();
                return MoveResult.GeneratingPath;
            }

            double distanceToNextHop = location.Distance3D(CurrentPath.Current);

            if (distanceToNextHop >= this.PathPrecision)
            {
                //Navigator.PlayerMover.MoveTowards(CurrentPath.Current);
                playerMover.MoveTowards(CurrentPath.Current);
                return MoveResult.Moved;
            }

            if (!CurrentPath.Next())
            {
                Logging.WriteDiagnostic(Colors.DeepSkyBlue,
                    "Navigation reached current destination. Within " + location.Distance(this.finalDestination));
                this.Clear();

                return MoveResult.ReachedDestination;
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = string.Concat(" (", name, ")");
            }

            var objArray = new object[]
                               {
                                   "Moving to next hop: ", CurrentPath.Current, name, "D: ",
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
