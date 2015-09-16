namespace ExBuddy.OrderBotTags.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    public static class Condition
    {
        private static readonly ConcurrentDictionary<int, ConditionTimer> Timers = new ConcurrentDictionary<int, ConditionTimer>();
        public static bool TrueFor(int id, TimeSpan span)
        {
            ConditionTimer timer;
            if (Timers.TryGetValue(id, out timer))
            {
                if (timer.TimeSpan != span)
                {
                    Timers[id] = new ConditionTimer(span);
                    timer.Timer.Dispose();
                    return true;
                }
                return timer.IsValid;
            }

            Timers[id] = new ConditionTimer(span);

            return true;
        }
    }

    public class ConditionTimer
    {
        public Timer Timer { get; private set; }

        public TimeSpan TimeSpan { get; private set; }

        private bool isValid = true;
        public bool IsValid
        {
            get
            {
                if(!this.isValid)
                {
                    this.Timer.Change(1,-1);
                    return false;
                }

                return this.isValid;
            }
        }

        public ConditionTimer(TimeSpan timeSpan)
        {
            this.TimeSpan = timeSpan;
            this.Timer = new Timer(ToggleValid, this, timeSpan, TimeSpan.FromMilliseconds(-1));
        }

        private static void ToggleValid(object context)
        {
            var _this = context as ConditionTimer;
            if (_this != null)
            {
                _this.isValid = !_this.isValid;
                _this.Timer.Change(_this.TimeSpan, TimeSpan.FromMilliseconds(-1));
            }
        }
    }
}
