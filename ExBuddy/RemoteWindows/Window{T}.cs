namespace ExBuddy.RemoteWindows
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using Buddy.Coroutines;

    using ExBuddy.Enums;
    using ExBuddy.Helpers;

    using ff14bot;
    using ff14bot.Behavior;
    using ff14bot.Helpers;
    using ff14bot.Managers;

    public abstract class Window<T> where T : Window<T>, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        private static Action updateWindows;

        static Window()
        {
            updateWindows = RaptureAtkUnitManager.Update;
            TreeRoot.OnStart += TreeRootOnStart;
            TreeRoot.OnStop += TreeRootOnStop;
        }

        static void TreeRootOnStop(ff14bot.AClasses.BotBase bot)
        {
            updateWindows = RaptureAtkUnitManager.Update;
        }

        static void TreeRootOnStart(ff14bot.AClasses.BotBase bot)
        {
            if (bot.PulseFlags.HasFlag(PulseFlags.Windows))
            {
                updateWindows = () => { };
            }
        } 

        private AtkAddonControl control;

        protected Window(string name)
        {
            Name = name;
            control = RaptureAtkUnitManager.GetWindowByName(name);
        }

        public static bool IsOpen
        {
            get
            {
                return new T().Control != null;
            }
        }

        public static AtkAddonControl AtkAddonControl
        {
            get
            {
                return new T().Control;
            }
        }

        public static bool Close()
        {
            return new T().CloseInstance() == SendActionResult.Success;
        }

        public static async Task<bool> CloseGently(byte maxTicks = 10, ushort interval = 200)
        {
            return await new T().CloseInstanceGently(maxTicks, interval);
        }

        public bool IsValid
        {
            get
            {
                return Control != null && Control.IsValid;
            }
        }

        public string Name { get; private set; }

        public virtual AtkAddonControl Control
        {
            get
            {
                return control ?? Refresh().control;
            }
        }

        public SendActionResult CloseInstance()
        {
            Logging.Write(Colors.DarkKhaki, "ExBuddy: Attempting to close the '{0}' window", Name);

            return TrySendAction(1, 3, uint.MaxValue);
        }

        public async Task<bool> CloseInstanceGently(byte maxTicks = 10, ushort interval = 200)
        {
            if (!IsValid)
            {
                return true;
            }

            await Wait(interval);

            if (CloseInstance() == SendActionResult.Success)
            {
                return true;
            }

            await Wait(interval);

            var result = SendActionResult.None;
            var ticks = 0;
            while (result != SendActionResult.Success && ticks++ < maxTicks && IsValid && Behaviors.ShouldContinue)
            {
                if (result == SendActionResult.InvalidWindow)
                {
                    return true;
                }

                result = CloseInstance();
                await Wait(interval);
            }

            return result > SendActionResult.InjectionError;
        }

        public SendActionResult TrySendAction(int pairCount, params uint[] param)
        {
            return Control.TrySendAction(pairCount, param);
        }

        public T Refresh()
        {
            updateWindows();
            control = RaptureAtkUnitManager.GetWindowByName(Name);
            return (T)this;
        }

        public async Task<bool> Refresh(int timeoutMs)
        {
            return await Coroutine.Wait(timeoutMs, () => Refresh().IsValid);
        }

        protected async Task Wait(int interval)
        {
            if (interval <= 33)
            {
                await Coroutine.Yield();
            }
            else
            {
                await Coroutine.Sleep(interval);
            }
        }
    }
}
