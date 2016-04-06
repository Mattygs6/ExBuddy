namespace ExBuddy.Plugins.RetainerMaid
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Configuration;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using Buddy.Coroutines;
	using ExBuddy.Attributes;
	using ExBuddy.Helpers;
	using ff14bot;
	using ff14bot.Behavior;
	using ff14bot.Enums;
	using ff14bot.Helpers;
	using ff14bot.Interfaces;
	using ff14bot.Managers;
	using ff14bot.RemoteWindows;
	using TreeSharp;

	public class BagSlotSnapshot
	{
		public BagSlotSnapshot(BagSlot slot) {}

		public InventoryBagId BagId { get; set; }
	}

	public class Retainer
	{
		public static readonly InventoryBagId[] BagIds =
		{
			InventoryBagId.Retainer_Page1, InventoryBagId.Retainer_Page2,
			InventoryBagId.Retainer_Page3, InventoryBagId.Retainer_Page4, InventoryBagId.Retainer_Page5,
			InventoryBagId.Retainer_Page6, InventoryBagId.Retainer_Page7
		};

		public readonly IList<BagSlot> BagSlots;

		public Retainer(int index)
		{
			Index = index;
			BagSlots =
				new List<BagSlot>(
					InventoryManager.GetBagsByInventoryBagId(BagIds).SelectMany(bag => bag.Select(bagSlot => bagSlot)));
		}

		public int Index { get; set; }
	}

	[LoggerName("RetainerMaid")]
	public class RetainerMaid : ExBotPlugin<RetainerMaid>
	{
		private readonly Stopwatch checkStopwatch = new Stopwatch();

		private readonly IList<Retainer> retainers = new List<Retainer>(8);

		private Composite mainCoroutine;

		public override string ButtonText
		{
			get { return "Housekeeping?!?"; }
		}

		public override string Name
		{
			get { return Localization.Localization.RetainerMaid_PluginName; }
		}

		public override bool WantButton
		{
			get { return true; }
		}

		public override void OnButtonPress()
		{
			// go to summon object, interact, do the things.
			INavigationProvider navigator;
			if (!TreeRoot.IsRunning) {}

			// For now, going to just build the logic here!

			var coroutine = new Coroutine(() => Main());
			coroutine.Resume();

			while (coroutine != null && !coroutine.IsFinished)
			{
				Thread.Sleep(33);
				Pulsator.Pulse(PulseFlags.All);
				coroutine.Resume();
			}
		}

		public override void OnDisabled()
		{
			checkStopwatch.Reset();
			////TreeHooks.Instance.OnHooksCleared -= OnHooksCleared;
			////TreeHooks.Instance.RemoveHook("TreeStart", mainCoroutine);
		}

		public override void OnEnabled()
		{
			////TreeHooks.Instance.AddHook("TreeStart", mainCoroutine);
			////TreeHooks.Instance.OnHooksCleared += OnHooksCleared;
			checkStopwatch.Restart();
		}

		public override void OnInitialize()
		{
			mainCoroutine = new ActionRunCoroutine(ctx => Main());
		}

		private async Task<bool> Main()
		{
			if (!checkStopwatch.IsRunning)
			{
				checkStopwatch.Restart();
			}

			if (checkStopwatch.Elapsed < Condition.OneDay)
			{
				//return false;
			}

			checkStopwatch.Restart();

			var settings = RetainerMaidSettings.Instance;
			var mybags =
				InventoryManager.GetBagsByInventoryBagId(
					InventoryBagId.Bag1,
					InventoryBagId.Bag2,
					InventoryBagId.Bag3,
					InventoryBagId.Bag4).SelectMany(bag => bag.Select(bagSlot => bagSlot)).ToArray();

			var myFullStacks = new Stack<BagSlot>(mybags.Where(b => b.IsFullStack()));

			if (myFullStacks.Count < 1 /* settings.MinStacksForDeposit */)
			{
				return false;
			}

			var pLocation = Core.Player.Location;
			//HousingEventObjects don't have npcids and as such would be unuseable
			//var bell = GameObjectManager.GetObjectsByNPCId<EventObject>(2000401).OrderBy(r=>r.Distance2D(pLocation)).FirstOrDefault();
			var bell =
				GameObjectManager.GameObjects.Where(r => r.IsVisible && r.EnglishName == "Summoning Bell")
					.OrderBy(r => r.Distance2D(pLocation))
					.FirstOrDefault();

			if (bell == null)
			{
				Logger.Error(Localization.Localization.RetainerMaid_NoNearestSummoningBell);
				return false;
			}

			bell.Interact();

			await Coroutine.Wait(3000, () => SelectString.IsOpen);

			var retainerCount = SelectString.LineCount - 1;

			while (retainerCount-- > 0)
			{
				SelectString.ClickSlot((uint) retainerCount);

				await Coroutine.Wait(3000, () => Talk.DialogOpen);
				await Coroutine.Sleep(500);
				while (Talk.DialogOpen)
				{
					Talk.Next();
				}

				await Coroutine.Wait(3000, () => Talk.DialogOpen);
				await Coroutine.Sleep(500);
				while (Talk.DialogOpen)
				{
					Talk.Next();
				}

				await Coroutine.Wait(5000, () => SelectString.IsOpen);

				retainers.Add(new Retainer(retainerCount));

				SelectString.ClickSlot(uint.MaxValue);

				await Coroutine.Wait(5000, () => !SelectString.IsOpen);
				await Coroutine.Wait(3000, () => Talk.DialogOpen);
				await Coroutine.Sleep(500);
				while (Talk.DialogOpen)
				{
					Talk.Next();
				}

				await Coroutine.Sleep(2000);
				bell.Interact();

				await Coroutine.Wait(5000, () => SelectString.IsOpen);
			}

			var retainer = retainers.FirstOrDefault(r => r.BagSlots.Any(bs => !bs.IsFilled));
			if (retainer == null)
			{
				return false;
			}

			var openSlots = new Stack<BagSlot>(retainer.BagSlots.Where(bs => !bs.IsFilled));

			SelectString.ClickSlot((uint) retainer.Index);
			await Coroutine.Wait(2000, () => !SelectString.IsOpen);
			await Coroutine.Wait(2000, () => SelectString.IsOpen);

			BagSlot slot;
			while ((slot = myFullStacks.Pop()) != null)
			{
				var openSlot = openSlots.Pop();
				if (openSlot == null)
				{
					return false;
				}

				slot.Move(openSlot);

				await Coroutine.Yield();
			}

			return false;
		}

		private void OnHooksCleared(object sender, EventArgs args)
		{
			////TreeHooks.Instance.AddHook("TreeStart", mainCoroutine);
		}

		public class RetainerMaidSettings : JsonSettings
		{
			private static RetainerMaidSettings instance;

			// ReSharper disable once UnusedParameter.Local
			public RetainerMaidSettings(string path)
				: base(Path.Combine(JsonSettings.CharacterSettingsDirectory, "RetainerMaid.json")) {}

			[Setting]
			[Category]
			[DefaultValue(false)]
			[DisplayName("Deposit Full Stacks")]
			[Description("If you have 99/99 of an item, it will try to deposit it into open spots your retainer has.")]
			public bool DepositFullStacks { get; set; }

			public static RetainerMaidSettings Instance
			{
				get { return instance ?? (instance = new RetainerMaidSettings("RetainerMaidSettings")); }
			}

			[Setting]
			[Category]
			[DefaultValue(10)]
			[DisplayName("Minimum Stacks")]
			[Description("The number of stacks before it attempts to deposit")]
			public int MinStacksForDeposit { get; set; }
		}
	}
}