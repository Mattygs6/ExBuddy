namespace ExBuddy.OrderBotTags.Behaviors
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Media;

	using Buddy.Coroutines;

	using Clio.Utilities;
	using Clio.XmlEngine;

	using ExBuddy.Attributes;
	using ExBuddy.Helpers;
	using ExBuddy.Windows;

	using ff14bot;
	using ff14bot.Behavior;
	using ff14bot.Helpers;
	using ff14bot.Managers;
	using ff14bot.RemoteWindows;

	using TreeSharp;

	[LoggerName("ExGuildLeve")]
	[XmlElement("ExTurnInGuildLeve")]
	public class ExTurnInGuildLeveTag : ExProfileBehavior
	{
		private readonly Stopwatch interactTimeout = new Stopwatch();

		private bool checkedTransport;

		private uint iconStringIndex = 9001;

		[DefaultValue(true)]
		[XmlAttribute("AcceptTransport")]
		public bool AcceptTransport { get; set; }

		[XmlAttribute("HqOnly")]
		public bool HqOnly { get; set; }

		[XmlAttribute("NqOnly")]
		public bool NqOnly { get; set; }

		[XmlAttribute("NpcId")]
		public uint NpcId { get; set; }

		[XmlAttribute("NpcLocation")]
		public Vector3 NpcLocation { get; set; }

		[DefaultValue(60)]
		[XmlAttribute("Timeout")]
		public int Timeout { get; set; }

		[DefaultValue("Collect Reward.")]
		[XmlAttribute("CollectRewardText")]
		public string CollectRewardText { get; set; }

		[DefaultValue("Yes.")]
		[XmlAttribute("YesText")]
		public string YesText { get; set; }

		protected override Color Info
		{
			get
			{
				return Colors.Plum;
			}
		}

		protected override Composite CreateBehavior()
		{
			return new ActionRunCoroutine(ctx => Main());
		}

		private async Task<bool> Main()
		{
			if (interactTimeout.Elapsed.TotalSeconds > Timeout)
			{
				Logger.Error("Timeout while turning in leve.");
				isDone = true;
				return true;
			}

			if (!checkedTransport)
			{
				checkedTransport = true;

				StatusText = "Checking for transport window.";

				var selectYesnoCountWindow = new SelectYesnoCount();
				if (await selectYesnoCountWindow.Refresh(2000))
				{
					StatusText = "Selecting transport option.";

					if (AcceptTransport)
					{
						selectYesnoCountWindow.Yes();
						await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading);
						await Coroutine.Wait(System.Threading.Timeout.Infinite, () => !CommonBehaviors.IsLoading);
					}
					else
					{
						await selectYesnoCountWindow.CloseInstance();
					}

					return true;
				}
			}

			// Movement
			if (Me.Distance(NpcLocation) > 3.5)
			{
				StatusText = "Moving to Npc -> " + NpcId;

				await Behaviors.MoveTo(NpcLocation, radius: 3.4f, name: " NpcId: " + NpcId);
				return true;
			}

			if (!interactTimeout.IsRunning)
			{
				interactTimeout.Restart();
			}

			// Interact
			if (Core.Target == null && Me.Distance(NpcLocation) <= 3.5)
			{
				return await InteractWithNpc();
			}

			if (Talk.DialogOpen)
			{
				Talk.Next();
				return true;
			}

			if (SelectIconString.IsOpen)
			{
				if (iconStringIndex == 9001)
				{
					iconStringIndex = (uint)SelectIconString.Lines().Count - 1;
				}

				// We will just click the last quest and decrement until we have either no quests left or none to turn in.
				SelectIconString.ClickSlot(--iconStringIndex);
				await Coroutine.Sleep(500);

				if (iconStringIndex == uint.MaxValue)
				{
					Logger.Warn("We don't have any completed quests left to turn in.");

					isDone = true;
					return true;
				}

				return true;
			}

			if (SelectString.IsOpen)
			{
				var lines = SelectString.Lines();

				// If Collect Reward exists, we click that; otherwise we will click Close. (-1 as uint = uint.MaxValue)
				var index = (uint)lines.IndexOf(CollectRewardText, StringComparer.InvariantCultureIgnoreCase);

				if (index != uint.MaxValue)
				{
					Logger.Info("Collecting reward on {0} ET", WorldManager.EorzaTime);
					SelectString.ClickSlot(index);
					return true;
				}

				// If yes is an option, click it to turn in more items.(crafting)
				index = (uint)lines.IndexOf(YesText, StringComparer.InvariantCultureIgnoreCase);

				if (index != uint.MaxValue)
				{
					Logger.Info("Turning in more items on {0} ET", WorldManager.EorzaTime);
					SelectString.ClickSlot(index);
					return true;
				}

				Logger.Warn("No rewards left, turn-ins complete.");
				isDone = true;
				SelectString.ClickSlot(index);
				return true;
			}

			if (Request.IsOpen)
			{
				////TODO: Want to support 3 turn ins of the same item (armor pieces etc.)  Probably going to have to either find out how to check if there are multiple requests, or just check the hand over prop and try to find new items to turn in...but this is...
				var itemId = Memory.Request.CurrentItemId;

				IEnumerable<BagSlot> itemSlots = InventoryManager.FilledInventoryAndArmory
					.Where(bs => bs.RawItemId == itemId && !Blacklist.Contains((uint)bs.Pointer.ToInt32(), BlacklistFlags.Loot))
					.ToArray();

				if (HqOnly)
				{
					itemSlots = itemSlots.Where(bs => bs.IsHighQuality);
				}

				if (NqOnly)
				{
					itemSlots = itemSlots.Where(bs => !bs.IsHighQuality);
				}

				var item = itemSlots.FirstOrDefault();

				if (item == null)
				{
					Logger.Warn("No items to turn in. Settings -> HqOnly: {0}, NqOnly: {1}, ItemId: {2}", HqOnly, NqOnly, itemId);
					isDone = true;
					return true;
				}

				StatusText = "Turning in items";

				var isHq = item.IsHighQuality;
				var itemName = item.EnglishName;
				var requestAttempts = 0;
				while (Request.IsOpen && requestAttempts++ < 5 && Behaviors.ShouldContinue && item.Item != null)
				{
					item.Handover();

					await Coroutine.Wait(1000, () => Request.HandOverButtonClickable);

					if (Request.HandOverButtonClickable)
					{
						Request.HandOver();

						if (isHq)
						{
							await Coroutine.Wait(2000, () => !Request.IsOpen && SelectYesno.IsOpen);
						}
						else
						{
							await Coroutine.Wait(2000, () => !Request.IsOpen);
						}
					}
				}

				if (Request.IsOpen)
				{
					Logger.Warn("We can't turn in Name: {0}, Count: {1}, SlotId: 0x{2}", itemName, item.Count, item.Pointer.ToString("X8"));

					Blacklist.Add(
						(uint)item.Pointer.ToInt32(),
						BlacklistFlags.Loot,
						TimeSpan.FromMinutes(5),
						"Don't turn in this item for 5 minutes");

					return true;
				}

				if (SelectYesno.IsOpen)
				{
					SelectYesno.ClickYes();
					Logger.Info("Turned in HQ {0} on {1} ET", itemName, WorldManager.EorzaTime);
				}
				else
				{
					Logger.Info("Turned in {0} on {1} ET", itemName, WorldManager.EorzaTime);
				}

				await Coroutine.Wait(2000, () => JournalResult.IsOpen);
				return true;
			}

			if (JournalResult.IsOpen)
			{
				await Coroutine.Wait(2000, () => JournalResult.ButtonClickable);
				JournalResult.Complete();

				await Coroutine.Wait(2000, () => !JournalResult.IsOpen);
				return true;
			}

			Logger.Info("Looks like no windows are open, lets clear our target and try again.");
			Me.ClearTarget();
			return true;
		}

		protected override void DoReset()
		{
			interactTimeout.Reset();
			checkedTransport = false;
			iconStringIndex = 9001;
		}

		protected override void OnDone()
		{
			interactTimeout.Stop();

			if (SelectYesno.IsOpen)
			{
				SelectYesno.ClickNo();
			}

			if (Request.IsOpen)
			{
				Request.Cancel();
			}

			if (JournalResult.IsOpen)
			{
				JournalResult.Decline();
			}

			if (SelectIconString.IsOpen)
			{
				SelectIconString.ClickSlot(uint.MaxValue);
			}
		}

		private async Task<bool> InteractWithNpc()
		{
			var ticks = 0;
			while (ticks++ < 3 && !SelectIconString.IsOpen && !SelectString.IsOpen && !Request.IsOpen && !JournalResult.IsOpen
					&& Behaviors.ShouldContinue)
			{
				GameObjectManager.GetObjectByNPCId(NpcId).Interact();

				await Coroutine.Wait(1000, () => Talk.DialogOpen);

				while (Talk.DialogOpen && Behaviors.ShouldContinue)
				{
					Talk.Next();
					await Coroutine.Yield();
				}

				await Coroutine.Wait(2000, () => SelectIconString.IsOpen || SelectString.IsOpen || Request.IsOpen || JournalResult.IsOpen);
			}

			if (ticks > 3)
			{
				Logger.Warn("Looks like we don't have any quests to turn in.");
				isDone = true;
				return true;
			}

			return true;
		}
	}
}
