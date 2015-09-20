namespace ExBuddy.Navigation
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using Buddy.Coroutines;

    using Clio.Utilities;

    using ExBuddy.Attributes;
    using ExBuddy.Helpers;
    using ExBuddy.Interfaces;
    using ExBuddy.Logging;

    using ff14bot;
    using ff14bot.Behavior;
    using ff14bot.Interfaces;
    using ff14bot.Managers;
    using ff14bot.Navigation;
    using ff14bot.NeoProfiles;
    using ff14bot.Settings;

    [LoggerName("FlightMover")]
    public class FlightEnabledSlideMover : LogColors, IFlightEnabledPlayerMover
    {
        protected readonly Logger Logger;

        private readonly Stopwatch landingStopwatch = new Stopwatch();

        private readonly Stopwatch totalLandingStopwatch = new Stopwatch();

        private readonly Stopwatch takeoffStopwatch = new Stopwatch();

        private Task landingTask;

        private Task takeoffTask;

        private Coroutine coroutine;

        private Coroutine landingCoroutine;

        private bool isTakingOff;

        private bool isLanding;

        private bool disposed;

        private static Func<Vector3, bool> shouldFlyToFunc = ShouldFlyInternal;

        private readonly IPlayerMover innerMover;

        private readonly IFlightMovementArgs flightMovementArgs;

        public FlightEnabledSlideMover(IPlayerMover innerMover, bool forceLanding = false)
            : this(innerMover, new FlightMovementArgs { ForceLanding = forceLanding })
        {
        }

        public FlightEnabledSlideMover(IPlayerMover innerMover, IFlightMovementArgs flightMovementArgs)
        {
            if (flightMovementArgs == null)
            {
                throw new NullReferenceException("flightMovementArgs is null");
            }

            Logger = new Logger(this);
            this.innerMover = innerMover;
            this.flightMovementArgs = flightMovementArgs;

            GameEvents.OnMapChanged += GameEventsOnMapChanged;
        }

        void GameEventsOnMapChanged(object sender, EventArgs e)
        {
            ShouldFly = false;
            Logger.Info("Set default value for flying to false until we can determine if we can fly in this zone.");
        }

        public static explicit operator SlideMover(FlightEnabledSlideMover playerMover)
        {
            return playerMover.innerMover as SlideMover;
        }

        public IPlayerMover InnerMover
        {
            get
            {
                return innerMover;
            }
        }

        public IFlightMovementArgs FlightMovementArgs
        {
            get
            {
                return flightMovementArgs;
            }
        }

        protected internal bool ShouldFly { get; private set; }

        public override Color Info
        {
            get
            {
                return Colors.LightSkyBlue;
            }
        }

        public void MoveStop()
        {
            if (!isLanding)
            {
                innerMover.MoveStop();
            }

            if (!isLanding && (flightMovementArgs.ForceLanding || GameObjectManager.LocalPlayer.Location.IsGround(6)))
            {
                isLanding = true;
                ForceLanding();
            }
        }

        public void MoveTowards(Vector3 location)
        {
            if (ShouldFly && !MovementManager.IsFlying && !isTakingOff)
            {
                isTakingOff = true;
                EnsureFlying();
            }

            if (!isTakingOff)
            {
                innerMover.MoveTowards(location);
            }
        }

        public void EnsureFlying()
        {
            if (!MovementManager.IsFlying && Actionmanager.CanMount == 0)
            {
                if (!takeoffStopwatch.IsRunning)
                {
                    takeoffStopwatch.Restart();
                }

                if (takeoffTask == null)
                {
                    Logger.Info("Started Takeoff Task");
                    takeoffTask = Task.Factory.StartNew(
                        () =>
                            {
                                try
                                {
                                    while (!MovementManager.IsFlying && Behaviors.ShouldContinue)
                                    {
                                        if (coroutine == null || coroutine.IsFinished)
                                        {
                                            Logger.Info("Created new Takeoff Coroutine");
                                            coroutine = new Coroutine(() => CommonTasks.TakeOff());
                                        }

                                        if (!coroutine.IsFinished && !MovementManager.IsFlying
                                            && Behaviors.ShouldContinue)
                                        {
                                            Logger.Info("Resumed Takeoff Coroutine");
                                            coroutine.Resume();
                                        }

                                        Thread.Sleep(33);
                                    }
                                }
                                finally
                                {
                                    Logger.Info(
                                        "Takeoff took {0} ms or less",
                                        takeoffStopwatch.Elapsed);
                                    takeoffStopwatch.Reset();
                                    isTakingOff = false;
                                    takeoffTask = null;
                                }
                            });
                }
            }
            else
            {
                isTakingOff = false;
            }
        }

        public void ForceLanding()
        {
            if (MovementManager.IsFlying)
            {
                if (!landingStopwatch.IsRunning)
                {
                    landingStopwatch.Restart();
                }

                if (!totalLandingStopwatch.IsRunning)
                {
                    totalLandingStopwatch.Restart();
                }

                if (landingTask == null)
                {
                    Logger.Info("Started Landing Task");
                    landingTask = Task.Factory.StartNew(
                        () =>
                            {
                                try
                                {
                                    while (MovementManager.IsFlying && Behaviors.ShouldContinue)
                                    {

                                        if (landingStopwatch.ElapsedMilliseconds < 2000)
                                        {
                                            MovementManager.StartDescending();
                                        }
                                        else
                                        {
                                            if (totalLandingStopwatch.ElapsedMilliseconds > 20000)
                                            {
                                                Logger.Error("Landing failed. Passing back control.");
                                                return;
                                            }

                                            if (landingCoroutine == null || landingCoroutine.IsFinished)
                                            {
                                                var move = Core.Player.Location.AddRandomDirection2D(10).GetFloor(15);
                                                MovementManager.StopDescending();
                                                MovementManager.Jump();
                                                landingCoroutine =
                                                    new Coroutine(() => Behaviors.MoveToNoMount(move, false, 0.5f));
                                                Logger.Info(
                                                    "Created new Landing Unstuck Coroutine, moving to {0}",
                                                    move);
                                            }

                                            if (!landingCoroutine.IsFinished && MovementManager.IsFlying)
                                            {
                                                Logger.Info("Resumed Landing Unstuck Coroutine");
                                                landingCoroutine.Resume();
                                            }
                                            else
                                            {
                                                landingStopwatch.Restart();
                                            }
                                        }

                                        Thread.Sleep(33);
                                    }
                                }
                                finally
                                {
                                    Logger.Info(
                                        "Landing took {0} ms or less",
                                        totalLandingStopwatch.Elapsed);
                                    totalLandingStopwatch.Reset();
                                    landingStopwatch.Reset();
                                    isLanding = false;
                                    landingTask = null;
                                }
                            });
                }
            }
            else
            {
                isLanding = false;
            }
        }

        public bool CanFly
        {
            get
            {
                return WorldManager.CanFly;
            }
        }

        public bool ShouldFlyTo(Vector3 destination)
        {
            if (shouldFlyToFunc == null)
            {
                return false;
            }

            return CanFly && (ShouldFly = shouldFlyToFunc(destination));
        }

        public async Task SetShouldFlyAsync(Task<Func<Vector3, bool>> customShouldFlyToFunc)
        {
            shouldFlyToFunc = await customShouldFlyToFunc;
        }

        internal static bool ShouldFlyInternal(Vector3 destination)
        {
            return MovementManager.IsFlying
                    || (Actionmanager.CanMount == 0 && ((destination.Distance3D(GameObjectManager.LocalPlayer.Location)
                    >= CharacterSettings.Instance.MountDistance) || !destination.IsGround()));
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Navigator.PlayerMover = innerMover;
                ff14bot.NeoProfiles.GameEvents.OnMapChanged -= GameEventsOnMapChanged;
            }
        }
    }
}