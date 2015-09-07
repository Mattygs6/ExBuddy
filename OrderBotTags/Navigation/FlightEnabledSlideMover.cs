namespace ExBuddy.OrderBotTags.Navigation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using Buddy.Coroutines;

    using Clio.Utilities;

    using ExBuddy.OrderBotTags.Common;

    using ff14bot;
    using ff14bot.Behavior;
    using ff14bot.Helpers;
    using ff14bot.Interfaces;
    using ff14bot.Managers;
    using ff14bot.Navigation;
    using ff14bot.Settings;

    public interface IFlightEnabledPlayerMover : IPlayerMover, IDisposable
    {
        bool CanFly { get; }

        bool ShouldFlyTo(Vector3 destination);

        Task SetShouldFlyAsync(Task<Func<Vector3, bool>> shouldFlyToFunc);
    }

    public class FlightEnabledSlideMover : IFlightEnabledPlayerMover
    {
        private readonly object obj = new object();

        private readonly Stopwatch landingStopwatch = new Stopwatch();

        private readonly Stopwatch totalLandingStopwatch = new Stopwatch();

        private Task landingTask;

        private Coroutine coroutine;

        private Coroutine landingCoroutine;

        private bool disposed;

        protected internal bool ShouldFly { get; private set; }

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

            this.innerMover = innerMover;
            this.flightMovementArgs = flightMovementArgs;

            ff14bot.NeoProfiles.GameEvents.OnMapChanged += GameEventsOnMapChanged;
        }

        void GameEventsOnMapChanged(object sender, EventArgs e)
        {
            ShouldFly = false;
            Logging.Write("Set default value for flying to false until we can determine if we can fly in this zone.");
        }

        public static explicit operator SlideMover(FlightEnabledSlideMover playerMover)
        {
            return playerMover.innerMover as SlideMover;
        }

        public IPlayerMover InnerMover
        {
            get
            {
                return this.innerMover;
            }
        }

        public void MoveStop()
        {
            innerMover.MoveStop();
            if (flightMovementArgs.ForceLanding || GameObjectManager.LocalPlayer.Location.IsGround(6))
            {
                ForceLanding();
            }
        }

        public void MoveTowards(Vector3 location)
        {
            if (ShouldFly && !MovementManager.IsFlying)
            {
                EnsureFlying().Wait(2000);
            }

            innerMover.MoveTowards(location);
        }

        public async Task EnsureFlying()
        {
            if (!ff14bot.Managers.MovementManager.IsFlying)
            {
                lock (obj)
                {
                    if (!ff14bot.Managers.MovementManager.IsFlying)
                    {
                        if (coroutine == null || coroutine.IsFinished)
                        {
                            Logging.Write("Created new TakeOff Coroutine");
                            coroutine = new Coroutine(() => CommonTasks.TakeOff());
                        }

                        coroutine.Resume();
                        Logging.Write("Resumed TakeOff Coroutine");
                    }
                }
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
                    Logging.Write("Started Landing Task");
                    landingTask = Task.Factory.StartNew(
                        () =>
                        {
                            while (MovementManager.IsFlying)
                            {
                                Thread.Sleep(50);
                            }

                            Logging.Write(Colors.DeepSkyBlue, "Landing took {0} ms or less", totalLandingStopwatch.Elapsed);
                            totalLandingStopwatch.Reset();
                            landingStopwatch.Reset();

                            landingTask = null;
                        });
                }

                if (landingStopwatch.ElapsedMilliseconds < 1500)
                {
                    MovementManager.StartDescending();
                }
                else
                {
                    var move = Core.Player.Location.AddRandomDirection2D(5).GetFloor();
                    if (landingCoroutine == null || landingCoroutine.IsFinished)
                    {
                        MovementManager.StopDescending();
                        landingCoroutine = new Coroutine(() => Behaviors.MoveToNoMount(move, false, 0.5f));
                        Logging.Write("Created new Landing Unstuck Coroutine");
                    }

                    landingCoroutine.Resume();
                    Logging.Write("Resumed Landing Unstuck Coroutine");
                    landingStopwatch.Restart();
                }
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
            return (MovementManager.IsFlying
                    || destination.Distance3D(GameObjectManager.LocalPlayer.Location)
                    >= CharacterSettings.Instance.MountDistance) || !destination.IsGround();
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

    public static class AsyncHelper
    {
        /// <summary>
        /// Execute's an async Task<T> method which has a void return value synchronously
        /// </summary>
        /// <param name="task">Task<T> method to execute</param>
        public static void RunSync(Func<Task> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            synch.Post(async _ =>
            {
                try
                {
                    await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();

            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        /// <summary>
        /// Execute's an async Task<T> method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">Task<T> method to execute</param>
        /// <returns></returns>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default(T);
            synch.Post(async _ =>
            {
                try
                {
                    ret = await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext
        {
            private bool done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> items =
                new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (items)
                {
                    items.Enqueue(Tuple.Create(d, state));
                }
                workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (items)
                    {
                        if (items.Count > 0)
                        {
                            task = items.Dequeue();
                        }
                    }
                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                        }
                    }
                    else
                    {
                        workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}