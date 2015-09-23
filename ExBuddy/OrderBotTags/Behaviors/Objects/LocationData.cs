namespace ExBuddy.OrderBotTags.Behaviors.Objects
{
	using Clio.Utilities;

	public struct LocationData
	{
		public uint AetheryteId { get; set; }

		public ushort ZoneId { get; set; }

		public uint NpcId { get; set; }

		public Vector3 NpcLocation { get; set; }

		public uint ShopNpcId { get; set; }

		public Vector3 ShopNpcLocation { get; set; }
	}
}