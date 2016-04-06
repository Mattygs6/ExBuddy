namespace ExBuddy.OrderBotTags.Behaviors
{
	using System;
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
	using ExBuddy.Interfaces;
	using ExBuddy.Windows;
	using ff14bot;
	using ff14bot.RemoteWindows;

	[LoggerName("ExGuildLeve")]
	[XmlElement("ExPickupGuildLeve")]
	public class ExPickupGuildLeveTag : ExProfileBehavior, IInteractWithNpc
	{
		private readonly Stopwatch interactTimeout = new Stopwatch();

		private uint[] ids;

		[XmlAttribute("LeveIds")]
		public int[] LeveIds { get; set; }

		// Not doing enum so i can support other languages
		[XmlAttribute("LeveType")]
		public string LeveType { get; set; }

		[DefaultValue(30)]
		[XmlAttribute("Timeout")]
		public int Timeout { get; set; }

		protected override Color Info
		{
			get { return Colors.MediumPurple; }
		}

		private uint[] Ids
		{
			get { return ids ?? (ids = LeveIds.Select(Convert.ToUInt32).ToArray()); }
		}

		protected override void DoReset()
		{
			interactTimeout.Reset();
		}

		protected override async Task<bool> Main()
		{
			if (Talk.DialogOpen)
			{
				await HandleTalk();
				return true;
			}

			if (SelectYesno.IsOpen)
			{
				SelectYesno.ClickYes();
				await Coroutine.Yield();
				return true;
			}

			if (JournalResult.IsOpen)
			{
				await Coroutine.Wait(1000, () => JournalResult.ButtonClickable);
				JournalResult.Complete();
				await Coroutine.Yield();
				return true;
			}

			// Movement
			if (ExProfileBehavior.Me.Distance(Location) > 3.5)
			{
				StatusText = Localization.Localization.ExPickupGuildLeve_Move + NpcId;

				await Location.MoveTo(radius: 3.4f, name: " NpcId: " + NpcId);
				return true;
			}

			if (!interactTimeout.IsRunning)
			{
				interactTimeout.Restart();
			}

			// Interact
			if (Core.Target == null && ExProfileBehavior.Me.Distance(Location) <= 3.5)
			{
				await this.Interact();
				await Coroutine.Yield();
				return true;
			}

			if (SelectString.IsOpen)
			{
				if (interactTimeout.Elapsed.TotalSeconds > Timeout || GuildLeve.HasLeves(Ids) || GuildLeve.Allowances == 0)
				{
					SelectString.ClickSlot(uint.MaxValue);
					isDone = true;
					return true;
				}

				SelectString.ClickLineContains(LeveType);
				return true;
			}

			var guildLeveWindow = new GuildLeve();
			if (guildLeveWindow.IsValid)
			{
				if (interactTimeout.Elapsed.TotalSeconds > Timeout || GuildLeve.HasLeves(Ids) || GuildLeve.Allowances == 0)
				{
					await guildLeveWindow.CloseInstance();
					return true;
				}

				foreach (var leveId in Ids.Where(id => !GuildLeve.HasLeve(id)))
				{
					if (GuildLeve.Allowances > 0)
					{
						StatusText = Localization.Localization.ExPickupGuildLeve_Pickup + leveId;
						Logger.Info(Localization.Localization.ExPickupGuildLeve_Pickup2 + leveId);

						await Coroutine.Sleep(1000);
						guildLeveWindow.AcceptLeve(leveId);
						await Coroutine.Yield(); // so our memory lock updates and level allowances change
					}
				}

				await Coroutine.Sleep(1000);

				return true;
			}

			// Interact if targetting but not null (if combat behaviors prevented the first one)
			if (ExProfileBehavior.Me.Distance(Location) <= 3.5)
			{
				await this.Interact();
				return true;
			}

			return true;
		}

		protected override void OnDone()
		{
			interactTimeout.Stop();
		}

		private async Task<bool> HandleTalk(int interval = 100)
		{
			await Coroutine.Wait(1000, () => Talk.DialogOpen);

			var ticks = 0;
			while (ticks++ < 50 && Talk.DialogOpen && Behaviors.ShouldContinue)
			{
				Talk.Next();
				await Coroutine.Sleep(interval);
			}

			return await WaitForOpenWindow();
		}

		private async Task<bool> WaitForOpenWindow()
		{
			return
				await
					Coroutine.Wait(3000, () => SelectIconString.IsOpen || SelectString.IsOpen || Request.IsOpen || JournalResult.IsOpen);
		}

		#region IInteractWithNpc Members

		[XmlAttribute("NpcLocation")]
		public Vector3 Location { get; set; }

		[XmlAttribute("NpcId")]
		public uint NpcId { get; set; }

		#endregion
	}
}