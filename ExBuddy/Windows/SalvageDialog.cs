using ExBuddy.Logging;

namespace ExBuddy.Windows
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;

	using ExBuddy.Agents;
	using ExBuddy.Enumerations;
	using ExBuddy.Helpers;

	using ff14bot;
	using ff14bot.Behavior;
	using ff14bot.Enums;
	using ff14bot.Managers;

	public sealed class SalvageDialog : Window<SalvageDialog>
	{
		public SalvageDialog()
			: base("SalvageDialog") { }

		public static SalvageDialog OpenDialog(BagSlot bagSlot)
		{
			var dialog = new SalvageDialog();
			if (dialog.Open(bagSlot))
			{
				return dialog;
			}

			return null;
		}

		public static async Task<bool> DesynthesizeAllItems(IEnumerable<BagSlot> bagSlots, ushort maxWait = 5000, bool desynthUniqueUntradeable = false)
		{
			foreach (var bagSlot in bagSlots)
			{
				if (!desynthUniqueUntradeable && bagSlot.Item != null && (bagSlot.Item.Unique || bagSlot.Item.Untradeable))
				{
					continue;
				}

				////var dialog = OpenDialog(bagSlot);
				////if (dialog == null)
				////{
				////	// TODO: Find conditions for this and put them in OpenDialog
				////	Logger.Instance.Info("Can not desynthesize {0}, the item is incompatible.", bagSlot.EnglishName);
				////	await Behaviors.Sleep(500);

				////	continue;
				////}

				////if (!await dialog.Refresh(maxWait))
				////{
				////	Logger.Instance.Error("An error has occured during desynthesis.");
				////	return false;
				////}

				////if (desynthUniqueUntradeable && ff14bot.RemoteWindows.SalvageDialog.CheckBoxVisible
				////	&& !ff14bot.RemoteWindows.SalvageDialog.UniqueUntradeableChecked)
				////{
				////	ff14bot.RemoteWindows.SalvageDialog.UniqueUntradeableChecked = true;
				////}

				////dialog.Desynthesize();
				////await dialog.Refresh(maxWait, false);

				////var result = new SalvageResult();
				////await result.Refresh(maxWait);
				////await result.CloseInstanceGently();

				while (bagSlot != null && bagSlot.Count > 0)
				{
					var result = await CommonTasks.Desynthesize(bagSlot, maxWait);
				    if (result.HasFlag(DesynthesisResult.Failure))
				    {
                        Logger.Instance.Error("An error has occured during desynthesis. Result was {0}",result);
				        break;
				    }

				}

				await Behaviors.Sleep(500);
			}

			return true;
		}

		public static async Task<bool> DesynthesizeByItemId(uint itemId, ushort maxWait = 5000, bool includeArmory = true, bool nqOnly = false, bool desynthesizeUniqueUntradeable = true)
		{
			var slots = includeArmory ? InventoryManager.FilledInventoryAndArmory : InventoryManager.FilledSlots;
			return await DesynthesizeAllItems(slots.Where(i => i.RawItemId == itemId && (!nqOnly || i.TrueItemId == itemId)), maxWait, desynthesizeUniqueUntradeable);
		}

		public static async Task<bool> DesynthesizeByRepairClass(
			ClassJobType classJobType,
			ushort maxWait = 5000,
			bool includeArmory = true, bool nqOnly = false, bool desynthesizeUniqueUntradeable = false)
		{
			var slots = includeArmory ? InventoryManager.FilledInventoryAndArmory : InventoryManager.FilledSlots;
			return await DesynthesizeAllItems(slots.Where(i => i.Item != null && classJobType == (ClassJobType)i.Item.RepairClass && (!nqOnly || (!i.IsHighQuality && !i.IsCollectable))), maxWait, desynthesizeUniqueUntradeable);
		}

		public bool Open(BagSlot bagSlot)
		{
			if (bagSlot != null && bagSlot.IsFilled && bagSlot.CanDesynthesize)
			{
				var item = bagSlot.Item;
				if (item != null)
				{
					lock (Core.Memory.Executor.AssemblyLock)
					{
						var memory = Core.Memory;
						var addr = memory.ImageBase + 0x006609E0;
						object[] pointer = { Desynthesis.Instance.Pointer, bagSlot.Pointer, 174 };
						memory.CallInjected(addr, CallingConvention.ThisCall, pointer);
					}

					return true;
				}
			}

			return false;
		}

		public SendActionResult Desynthesize()
		{
			return Control.TrySendAction(1, 3, 0);
		}
	}
}
