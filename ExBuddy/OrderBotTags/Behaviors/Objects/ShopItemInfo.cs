namespace ExBuddy.OrderBotTags.Behaviors.Objects
{
    using ff14bot.Managers;

    public struct ShopItemInfo
    {
        public uint Index { get; set; }
        public ShopType ShopType { get; set; }
        public uint ItemId { get; set; }
        public ushort Cost { get; set; }
        public byte Yield { get; set; }

        public Item ItemData
        {
            get
            {
                return DataManager.ItemCache[this.ItemId];
            }
        }
    }
}