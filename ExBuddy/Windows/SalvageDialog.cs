namespace ExBuddy.Windows
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using ExBuddy.Helpers;
	using ExBuddy.Logging;
	using ff14bot.Behavior;
	using ff14bot.Enums;
	using ff14bot.Managers;

	public sealed class SalvageDialog : Window<SalvageDialog>
	{
		public SalvageDialog()
			: base("SalvageDialog") {}

		public static async Task<bool> DesynthesizeAllItems(
			IEnumerable<BagSlot> bagSlots,
			ushort maxWait = 5000,
			bool desynthUniqueUntradeable = false)
		{
			foreach (var bagSlot in bagSlots)
			{
				if (!desynthUniqueUntradeable && bagSlot.Item != null && (bagSlot.Item.Unique || bagSlot.Item.Untradeable))
				{
					continue;
				}

				if (bagSlot != null)
				{
					var startingId = bagSlot.TrueItemId;

					//Check to make sure the bagslots contents doesn't change
					while (bagSlot.TrueItemId == startingId && bagSlot.Count > 0)
					{
						var result = await CommonTasks.Desynthesize(bagSlot, maxWait);
						if (result.HasFlag(DesynthesisResult.Failure))
						{
							Logger.Instance.Error(Localization.Localization.SalvageDialog, result);
							break;
						}
					}
				}

				await Behaviors.Sleep(500);
			}

			return true;
		}

		public static async Task<bool> DesynthesizeByItemId(
			uint itemId,
			ushort maxWait = 5000,
			bool includeArmory = true,
			bool nqOnly = false,
			bool desynthesizeUniqueUntradeable = true)
		{
			var slots = includeArmory ? InventoryManager.FilledInventoryAndArmory : InventoryManager.FilledSlots;
			return
				await
					DesynthesizeAllItems(
						slots.Where(i => i.RawItemId == itemId && (!nqOnly || i.TrueItemId == itemId)),
						maxWait,
						desynthesizeUniqueUntradeable);
		}

		public static async Task<bool> DesynthesizeByRepairClass(
			ClassJobType classJobType,
			ushort maxWait = 5000,
			bool includeArmory = true,
			bool nqOnly = false,
			bool desynthesizeUniqueUntradeable = false)
		{
			var slots = includeArmory ? InventoryManager.FilledInventoryAndArmory : InventoryManager.FilledSlots;
			return
				await
					DesynthesizeAllItems(
						slots.Where(
							i =>
								i.Item != null && classJobType == (ClassJobType) i.Item.RepairClass
								&& (!nqOnly || (!i.IsHighQuality && !i.IsCollectable))),
						maxWait,
						desynthesizeUniqueUntradeable);
		}
	}
}