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
	using ff14bot.Enums;
	using ff14bot.Managers;

	[LoggerName("ExSalvage")]
	[XmlElement("ExSalvage")]
	[XmlElement("ExDesynthesize")]
	public class ExSalvageTag : ExProfileBehavior
	{
		[DefaultValue("True")]
		[XmlAttribute("Condition")]
		public string Condition { get; set; }

		[XmlAttribute("ItemIds")]
		public int[] ItemIds { get; set; }

		[XmlAttribute("NqOnly")]
		public bool NqOnly { get; set; }

		[XmlAttribute("RepairClass")]
		public ClassJobType RepairClass { get; set; }

		[DefaultValue(true)]
		[XmlAttribute("IncludeArmory")]
		public bool IncludeArmory { get; set; }

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
				Logger.Info("Did not meet the condition to salvage, [{0}]", Condition);
				return isDone = true;
			}

			var ticks = 0;
			while(MovementManager.IsFlying && ticks++ < 5 && Behaviors.ShouldContinue)
			{
				MovementManager.StartDescending();
				await Coroutine.Wait(500, () => !MovementManager.IsFlying);
			}

			if (ticks > 5)
			{
				Logger.Error("Unable to land, can't salvage unless we land!");
				return isDone = true;
			}

			await CommonTasks.StopAndDismount();

			if (RepairClass > ClassJobType.Thaumaturge && RepairClass < ClassJobType.Miner)
			{
				await SalvageDialog.DesynthesizeByRepairClass(RepairClass, (ushort)MaxWait, IncludeArmory, NqOnly);
			}

			if (ItemIds != null && ItemIds.Length > 0)
			{
				foreach (var id in ItemIds)
				{
					await SalvageDialog.DesynthesizeByItemId((uint)id, (ushort)MaxWait, IncludeArmory, NqOnly);
				}
			}

			return isDone = true;
		}
	}
}
