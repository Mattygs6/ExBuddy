namespace ExBuddy.OrderBotTags.Behaviors
{
	using System.ComponentModel;
	using System.Threading.Tasks;

	using Buddy.Coroutines;

	using Clio.Utilities;
	using Clio.XmlEngine;

	using ExBuddy.Attributes;
	using ExBuddy.Helpers;
	using ExBuddy.Windows;

	using ff14bot.Behavior;
	using ff14bot.Managers;
	using ff14bot.Navigation;

	[LoggerName("ExPurify")]
	[XmlElement("ExPurify")]
	[XmlElement("ExReduce")]
	public class ExPurifyTag : ExProfileBehavior
	{
		[DefaultValue("True")]
		[XmlAttribute("Condition")]
		public string Condition { get; set; }

		[DefaultValue(5000)]
		[XmlAttribute("MaxWait")]
		public int MaxWait { get; set; }

		protected override void OnStart()
		{
			MaxWait = MaxWait.Clamp(1000, 10000);
		}

		protected async override Task<bool> Main()
		{
			if (!ScriptManager.GetCondition(Condition)())
			{
				Logger.Info("Did not meet the condition to Purify, [{0}]", Condition);
				return isDone = true;
			}

			await Behaviors.Wait(2000, () => !Gathering.IsOpen);
			await Behaviors.Wait(2000, () => !GatheringMasterpiece.IsOpen);

			Navigator.Stop();

			var ticks = 0;
			while(MovementManager.IsFlying && ticks++ < 5 && Behaviors.ShouldContinue)
			{
				MovementManager.StartDescending();
				await Coroutine.Wait(500, () => !MovementManager.IsFlying);
			}

			if (ticks > 5)
			{
				Logger.Error("Unable to land, can't reduce unless we land!");
				return isDone = true;
			}

			await CommonTasks.StopAndDismount();

			if (await Coroutine.Wait(
				MaxWait,
				() =>
					{
						if (!Me.IsMounted)
						{
							return true;
						}

						Actionmanager.Dismount();
						return false;
					}))
			{
				await PurifyDialog.ReduceAllItems(InventoryManager.FilledSlots, (ushort)MaxWait);
			}
			else
			{
				Logger.Error("Could not dismount.");
			}

			return isDone = true;
		}
	}
}
