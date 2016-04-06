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

		[XmlAttribute("IncludeArmory")]
		public bool IncludeArmory { get; set; }

		[XmlAttribute("ItemIds")]
		public int[] ItemIds { get; set; }

		[DefaultValue(5000)]
		[XmlAttribute("MaxWait")]
		public int MaxWait { get; set; }

		[XmlAttribute("NqOnly")]
		public bool NqOnly { get; set; }

		[XmlAttribute("RepairClass")]
		public ClassJobType RepairClass { get; set; }

		protected override async Task<bool> Main()
		{
			if (!ScriptManager.GetCondition(Condition)())
			{
				Logger.Info(Localization.Localization.ExSalvage_GetCondition, Condition);
				return isDone = true;
			}

			var ticks = 0;
			while (MovementManager.IsFlying && ticks++ < 5 && Behaviors.ShouldContinue)
			{
				MovementManager.StartDescending();
				await Coroutine.Wait(500, () => !MovementManager.IsFlying);
			}

			if (ticks > 5)
			{
				Logger.Error(Localization.Localization.ExSalvage_Land);
				return isDone = true;
			}

			await CommonTasks.StopAndDismount();

			if (RepairClass > ClassJobType.Thaumaturge && RepairClass < ClassJobType.Miner)
			{
				await SalvageDialog.DesynthesizeByRepairClass(RepairClass, (ushort) MaxWait, IncludeArmory, NqOnly);
			}

			if (ItemIds != null && ItemIds.Length > 0)
			{
				foreach (var id in ItemIds)
				{
					await SalvageDialog.DesynthesizeByItemId((uint) id, (ushort) MaxWait, IncludeArmory, NqOnly);
				}
			}

			return isDone = true;
		}

		protected override void OnStart()
		{
			MaxWait = MaxWait.Clamp(1000, 10000);
		}
	}
}