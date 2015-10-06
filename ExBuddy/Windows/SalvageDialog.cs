namespace ExBuddy.Windows
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;

	using ExBuddy.Agents;
	using ExBuddy.Enumerations;
	using ExBuddy.Helpers;
	using ExBuddy.Logging;

	using ff14bot;
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

		public static async Task<bool> DesynthesizeAllItems(IEnumerable<BagSlot> bagSlots, ushort maxWait = 5000)
		{
			foreach (var bagSlot in bagSlots)
			{
				var dialog = OpenDialog(bagSlot);
				if (dialog == null)
				{
					// TODO: Find conditions for this and put them in OpenDialog
					Logger.Instance.Info("Can not desynthesize {0}, the item is incompatible.", bagSlot.EnglishName);
					await Behaviors.Sleep(500);

					continue;
				}

				await dialog.Refresh(maxWait);

				// TODO: Find some sort of way to determine if we don't meet the requirements
				if (false)
				{
					Logger.Instance.Info("Can not desynthesize {0}, we do not meet the requirements", bagSlot.EnglishName);
					await dialog.CloseInstanceGently();

					await Behaviors.Sleep(500);

					continue;
				}

				dialog.Desynthesize();
				await dialog.Refresh(maxWait, false);

				var result = new SalvageResult();
				await result.Refresh(maxWait);
				await result.CloseInstanceGently();

				await Behaviors.Sleep(500);
			}

			return true;
		}

		public static async Task<bool> DesynthesizeByItemId(uint itemId, ushort maxWait = 5000)
		{
			return await DesynthesizeAllItems(InventoryManager.FilledInventoryAndArmory.Where(i => i.RawItemId == itemId));
		}

		public bool Open(BagSlot bagSlot)
		{
			if (bagSlot != null && bagSlot.IsFilled)
			{
				var item = bagSlot.Item;
				// TODO: need some sort of check to see if it is desynthable
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
