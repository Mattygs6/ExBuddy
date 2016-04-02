namespace ExBuddy.OrderBotTags.Behaviors.Objects
{
	using ff14bot.Managers;

	public struct ShopItemInfo
	{
		public ushort Cost { get; set; }

		public uint Index { get; set; }

		public Item ItemData
		{
			get { return DataManager.ItemCache[ItemId]; }
		}

		public uint ItemId { get; set; }

		public ShopType ShopType { get; set; }

		public byte Yield { get; set; }
	}
}