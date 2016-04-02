namespace ExBuddy.Windows
{
	using System;
	using System.Threading.Tasks;
	using Buddy.Coroutines;
	using ExBuddy.Enumerations;
	using ExBuddy.Helpers;
	using ExBuddy.Logging;
	using ff14bot;
	using ff14bot.AClasses;
	using ff14bot.Behavior;
	using ff14bot.Managers;

	public abstract class Window<TWindow>
		where TWindow : Window<TWindow>, new()
	{
		// ReSharper disable once StaticMemberInGenericType
		private static Action updateWindows;

		private AtkAddonControl control;

		static Window()
		{
			updateWindows = RaptureAtkUnitManager.Update;
			TreeRoot.OnStart += TreeRootOnStart;
			TreeRoot.OnStop += TreeRootOnStop;
		}

		protected Window(string name)
		{
			Name = name;
			control = RaptureAtkUnitManager.GetWindowByName(name);
		}

		public static AtkAddonControl AtkAddonControl
		{
			get { return new TWindow().Control; }
		}

		public virtual AtkAddonControl Control
		{
			get { return control ?? Refresh().control; }
		}

		public static bool IsOpen
		{
			get { return new TWindow().Control != null; }
		}

		public bool IsValid
		{
			get { return Control != null && Control.IsValid; }
		}

		public string Name { get; set; }

		public static void Close()
		{
			new TWindow().Control.TrySendAction(1, 3, uint.MaxValue);
		}

		public static async Task<bool> CloseGently(byte maxTicks = 10, ushort interval = 200)
		{
			return await new TWindow().CloseInstanceGently(maxTicks, interval);
		}

		public virtual async Task<SendActionResult> CloseInstance(ushort interval = 250)
		{
			await Behaviors.Sleep(interval/2);
#if RB_CN
            Logger.Instance.Verbose("尝试关闭 [{0}] 窗口", Name);
#else
            Logger.Instance.Verbose("Attempting to close the [{0}] window", Name);
#endif

            var result = TrySendAction(1, 3, uint.MaxValue);

			await Refresh(interval/2, false);

			if (result == SendActionResult.Success)
			{
				if (!IsValid)
				{
#if RB_CN
					Logger.Instance.Verbose(" [{0}] 窗口已关闭.", Name);
#else
                    Logger.Instance.Verbose("The [{0}] window has been closed.", Name);
#endif
                    return result;
				}
#if RB_CN
                Logger.Instance.Verbose("等待 {0} 毫秒,直到 [{1}] 窗口关闭", interval*2, Name);
#else
                Logger.Instance.Verbose("Waiting {0} ms for [{1}] window to close", interval*2, Name);
#endif
                await Refresh(interval*2, false);

				if (!IsValid)
				{
#if RB_CN
                    Logger.Instance.Verbose("[{0}] 窗口已关闭.", Name);
#else
                    Logger.Instance.Verbose("The [{0}] window has been closed.", Name);
#endif
                    return result;
				}
#if RB_CN
                Logger.Instance.Verbose("关闭 [{0}] 窗口时发生了不可预见的结果", Name);
#else
                Logger.Instance.Verbose("Unexpected result while closing [{0}]", Name);
#endif
                return SendActionResult.UnexpectedResult;
			}

			if (result == SendActionResult.InvalidWindow)
			{
#if RB_CN
                Logger.Instance.Verbose(" [{0}] 窗口不可用,可能窗口未打开或是自己关闭了.", Name);
#else
                Logger.Instance.Verbose("The [{0}] window was not valid, it was either not open or closed on its own.", Name);
#endif
            }

            return result;
		}

		public virtual async Task<bool> CloseInstanceGently(byte maxTicks = 10, ushort interval = 200)
		{
			if (!IsValid)
			{
				return true;
			}

			if (await CloseInstance(interval) == SendActionResult.Success)
			{
				if (!IsValid)
				{
					return true;
				}
			}

			await Behaviors.Sleep(interval);

			var result = SendActionResult.None;
			var ticks = 0;
			while (result != SendActionResult.Success && ticks++ < maxTicks && IsValid && Behaviors.ShouldContinue)
			{
				if (result == SendActionResult.InvalidWindow)
				{
					return true;
				}

				result = await CloseInstance(interval);
			}

			return result > SendActionResult.UnexpectedResult && !IsValid;
		}

		public TWindow Refresh()
		{
			updateWindows();
			control = RaptureAtkUnitManager.GetWindowByName(Name);
			return (TWindow) this;
		}

		public async Task<bool> Refresh(int timeoutMs, bool valid = true)
		{
			return await Coroutine.Wait(timeoutMs, () => Refresh().IsValid == valid);
		}

#if RB_X64
        public virtual SendActionResult TrySendAction(int pairCount, params ulong[] param)
#else
		public virtual SendActionResult TrySendAction(int pairCount, params uint[] param)
#endif

		{
			return Control.TrySendAction(pairCount, param);
		}

		private static void TreeRootOnStart(BotBase bot)
		{
			if (bot.PulseFlags.HasFlag(PulseFlags.Windows))
			{
				updateWindows = () => { };
			}
		}

		private static void TreeRootOnStop(BotBase bot)
		{
			updateWindows = RaptureAtkUnitManager.Update;
		}
	}
}