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

	public sealed class PurifyDialog : Window<PurifyDialog>
	{
		public PurifyDialog()
			: base("PurifyDialog") { }

		public static PurifyDialog OpenDialog(BagSlot bagSlot)
		{
			var dialog = new PurifyDialog();
			if (dialog.Open(bagSlot))
			{
				return dialog;
			}

			return null;
		}

		public static async Task<bool> ReduceAllItems(IEnumerable<BagSlot> bagSlots, ushort maxWait = 5000)
		{
			foreach (var bagSlot in bagSlots)
			{
				var dialog = OpenDialog(bagSlot);
				if (dialog == null)
				{
					Logger.Instance.Info("Can not reduce {0}, the item is incompatible.", bagSlot.EnglishName);
					await Behaviors.Sleep(500);

					continue;
				}

				await dialog.Refresh(maxWait);
				await Behaviors.Wait(maxWait, () => AetherialReduction.Instance.MaxPurity != 0);

				if (AetherialReduction.Instance.MaxPurity == 0)
				{
					Logger.Instance.Info("Can not reduce {0}, we do not meet the requirements or the item is not reducible", bagSlot.EnglishName);
					await dialog.CloseInstanceGently();

					await Behaviors.Sleep(500);

					continue;
				}

				dialog.Reduce();
				await dialog.Refresh(maxWait, false);

				var result = new PurifyResult();
				await result.Refresh(maxWait);
				await result.CloseInstanceGently();

				await Behaviors.Sleep(500);
			}

			return true;
		}

		public static async Task<bool> ReduceByItemId(uint itemId, ushort maxWait = 5000)
		{
			return await ReduceAllItems(InventoryManager.FilledSlots.Where(i => i.RawItemId == itemId && i.IsReducible()));
		}

		public bool Open(BagSlot bagSlot)
		{
			if (bagSlot != null && bagSlot.IsFilled)
			{
				// TODO: get mastahg to implement real IsReducible check
				if (!bagSlot.IsReducible())
				{
					return false;
				}

				var item = bagSlot.Item;
				
				if (item != null)
				{
					lock (Core.Memory.Executor.AssemblyLock)
					{
						var memory = Core.Memory;
						var addr = memory.ImageBase + 0x00660DB0;
						object[] pointer = { AetherialReduction.Instance.Pointer, bagSlot.Pointer, 249 };
						memory.CallInjected(addr, CallingConvention.ThisCall, pointer);
					}

					return true;
				}
			}

			return false;
		}

		public SendActionResult Reduce()
		{
			return Control.TrySendAction(1, 3, 0);
		}
	}
}
