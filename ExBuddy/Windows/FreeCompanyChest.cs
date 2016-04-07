namespace ExBuddy.Windows
{
	using System.Threading.Tasks;
	using ExBuddy.Enumerations;
	using ExBuddy.Helpers;

	public sealed class FreeCompanyChest : Window<FreeCompanyChest>
	{
		public FreeCompanyChest()
			: base("FreeCompanyChest") {}

		public SendActionResult CrystalsSection()
		{
			return SelectSection(1);
		}

		public SendActionResult GilSection()
		{
			return SelectSection(2);
		}

		public SendActionResult HistorySection()
		{
			return SelectSection(3);
		}

		public SendActionResult ItemsSection()
		{
			return SelectSection(0);
		}

		public SendActionResult RemoveItemBySlotIndex(byte index)
		{
			return TrySendAction(2, 1, 5, 1, index);
		}

		public async Task<bool> RemoveItemCountBySlotIndex(byte index, byte count, byte attempts = 10, ushort interval = 200)
		{
			var result = SendActionResult.None;
			var removeAttempts = 0;
			var inputNumericWindow = new InputNumeric();
			while (result != SendActionResult.Success && !inputNumericWindow.IsValid && removeAttempts++ < attempts
			       && Behaviors.ShouldContinue)
			{
				result = RemoveItemBySlotIndex(index);

				await inputNumericWindow.Refresh(interval);
			}

			if (removeAttempts > attempts)
			{
				return false;
			}

			result = SendActionResult.None;
			removeAttempts = 0;
			while (result != SendActionResult.Success && inputNumericWindow.IsValid && removeAttempts++ < attempts
			       && Behaviors.ShouldContinue)
			{
				result = inputNumericWindow.Count(count);

				await inputNumericWindow.Refresh(interval, false);
			}

			return !inputNumericWindow.IsValid;
		}

		public SendActionResult SelectItemTab(byte tab)
		{
			return TrySendAction(2, 0, 0, 1, tab);
		}

		public SendActionResult SelectSection(byte section)
		{
			return TrySendAction(2, 1, section, 0, 0);
		}
	}
}