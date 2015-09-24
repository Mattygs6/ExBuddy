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
	using ExBuddy.RemoteWindows;

	using ff14bot;
	using ff14bot.Managers;
	using ff14bot.RemoteWindows;

	using TreeSharp;

	[LoggerName("ExGuildLeve")]
	[XmlElement("ExPickupGuildLeve")]
	public class ExPickupGuildLeveTag : ExProfileBehavior
	{
		private readonly Stopwatch interactTimeout = new Stopwatch();

		[XmlAttribute("LeveIds")]
		public int[] LeveIds { get; set; }

		// Not doing enum so i can support other languages
		[XmlAttribute("LeveType")]
		public string LeveType { get; set; }

		[XmlAttribute("NpcId")]
		public uint NpcId { get; set; }

		[XmlAttribute("NpcLocation")]
		public Vector3 NpcLocation { get; set; }

		[DefaultValue(30)]
		[XmlAttribute("Timeout")]
		public int Timeout { get; set; }

		protected override Color Info
		{
			get
			{
				return Colors.MediumPurple;
			}
		}

		private uint[] Ids
		{
			get
			{
				return LeveIds.Select(Convert.ToUInt32).ToArray();
			}
		}

		protected override void DoReset()
		{
			interactTimeout.Reset();
		}

		protected override void OnDone()
		{
			interactTimeout.Stop();
		}

		protected override Composite CreateBehavior()
		{
			return new ActionRunCoroutine(ctx => Main());
		}

		private async Task<bool> Main()
		{
			if (Talk.DialogOpen)
			{
				Talk.Next();
				return true;
			}

			if (SelectYesno.IsOpen)
			{
				SelectYesno.ClickYes();
				return true;
			}

			if (JournalResult.IsOpen)
			{
				await Coroutine.Wait(1000, () => JournalResult.ButtonClickable);
				JournalResult.Complete();
				return true;
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
				GameObjectManager.GetObjectByNPCId(NpcId).Interact();
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
						StatusText = "Picking up Leve -> " + leveId;
						Logger.Info("Picking up Leve: " + leveId);

						await Coroutine.Sleep(1000);
						guildLeveWindow.AcceptLeve(leveId);
						await Coroutine.Yield(); // so our memory lock updates and level allowances change
					}
				}

				await Coroutine.Sleep(1000);

				return true;
			}

			// Interact if targetting but not null (if combat behaviors prevented the first one)
			if (Me.Distance(NpcLocation) <= 3.5)
			{
				GameObjectManager.GetObjectByNPCId(NpcId).Interact();
				return true;
			}

			return true;
		}
	}
}
