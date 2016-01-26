using ExBuddy.Offsets;

namespace ExBuddy.Windows
{
	using ExBuddy.Enumerations;

	using ff14bot;
	using ff14bot.Managers;

	public sealed class SelectYesNoItem : Window<SelectYesNoItem>
	{
		public SelectYesNoItem()
			: base("SelectYesNoItem") { }

		public Item Item
		{
			get
			{
				var itemId = ItemId;

				if (itemId.HasValue)
				{
					return DataManager.GetItem(itemId.Value);
				}

				return null;
			}
		}

		public uint? ItemId
		{
			get
			{
				if (IsValid)
				{
					return Core.Memory.Read<uint>(SelectYesNoItemOffsets.ItemIdPtr) % 500000;
				}

				return null;
			}
		}

		public uint? CollectabilityValue
		{
			get
			{
				if (IsValid)
				{
					return Core.Memory.Read<uint>(SelectYesNoItemOffsets.ItemIdPtr + SelectYesNoItemOffsets.CollectabilityValueOffset);
				}

				return null;
			}
		}

		public SendActionResult Yes()
		{
			return TrySendAction(1, 3, 0);
		}

		public SendActionResult No()
		{
			return TrySendAction(1, 3, 1);
		}
	}
}
