namespace ExBuddy.RemoteWindows
{
    using System.Threading.Tasks;
    using System.Windows.Media;

    using Buddy.Coroutines;

    using ExBuddy.Enums;
    using ExBuddy.Helpers;

    using ff14bot.Helpers;
    using ff14bot.Managers;

    public abstract class Window<T> where T : Window<T>, new()
    {
        private AtkAddonControl control;

        protected Window(string name)
        {
            Name = name;
            this.control = RaptureAtkUnitManager.GetWindowByName(name);
        }

        public static bool IsOpen
        {
            get
            {
                return new T().control != null;
            }
        }

        public static bool Close()
        {
            return new T().CloseInstance() == SendActionResult.Success;
        }

        public static async Task<bool> CloseGently(byte maxTicks = 10, byte interval = 200)
        {
            return await new T().CloseInstanceGently(maxTicks, interval);
        }

        public bool IsValid
        {
            get
            {
                return control != null && control.IsValid;
            }
        }

        public string Name { get; private set; }

        public AtkAddonControl Control
        {
            get
            {
                return control ?? (control = RaptureAtkUnitManager.GetWindowByName(Name));
            }
        }

        public SendActionResult CloseInstance()
        {
            Logging.Write(Colors.DarkKhaki, "ExBuddy: Attempting to close the '{0}' window", Name);

            return TrySendAction(1, 3, uint.MaxValue);
        }

        public async Task<bool> CloseInstanceGently(byte maxTicks = 10, byte interval = 200)
        {
            if (!IsValid)
            {
                return true;
            }

            if (CloseInstance() == SendActionResult.Success)
            {
                return true;
            }

            var result = SendActionResult.None;
            var ticks = 0;
            while (result != SendActionResult.Success && ticks++ < maxTicks && IsValid && Behaviors.ShouldContinue)
            {
                if (result == SendActionResult.InvalidWindow)
                {
                    return true;
                }

                result = CloseInstance();
                if (interval <= 33)
                {
                    await Coroutine.Yield();
                }
                else
                {
                    await Coroutine.Sleep(interval);
                }
            }

            return result > SendActionResult.InjectionError;
        }

        public SendActionResult TrySendAction(int pairCount, params uint[] param)
        {
            return this.control.TrySendAction(pairCount, param);
        }

        public T Refresh()
        {
            control = RaptureAtkUnitManager.GetWindowByName(Name);
            return (T)this;
        }

        public async Task<bool> Refresh(int timeoutMs)
        {
            return await Coroutine.Wait(timeoutMs, () => Refresh().IsValid);
        }
    }
}
