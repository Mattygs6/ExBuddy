namespace ExBuddy.Helpers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using Clio.Utilities;

    using ExBuddy.Logging;

    public static class Condition
    {
        static Condition()
        {
            AddNamespacesToScriptManager("ExBuddy", "ExBuddy.Helpers");
        }

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

        public static void AddNamespacesToScriptManager(params string[] param)
        {
            var field = typeof(ScriptManager)
                .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                .FirstOrDefault(f => f.FieldType == typeof(List<string>));

            if (field == null)
            {
                return;
            }

            try
            {
                var list = field.GetValue(null) as List<string>;
                if (list == null)
                {
                    return;
                }

                foreach (var ns in param)
                {
                    if (!list.Contains(ns))
                    {
                        list.Add(ns);
                        Logger.Instance.Info("Added namespace '{0}' to ScriptManager", ns);
                    }
                }
            }
            catch
            {
                Logger.Instance.Error("Failed to add namespaces to ScriptManager, this can cause issues with some profiles.");
            }
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
                if(!isValid)
                {
                    Timer.Change(1,-1);
                    return false;
                }

                return isValid;
            }
        }

        public ConditionTimer(TimeSpan timeSpan)
        {
            TimeSpan = timeSpan;
            Timer = new Timer(ToggleValid, this, timeSpan, TimeSpan.FromMilliseconds(-1));
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
