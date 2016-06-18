namespace ExBuddy.Data
{
	using ff14bot.Enums;

	using SQLite;

	public class MasterpieceSupplyDutyResult
	{
		[PrimaryKey]
		public uint Id { get; set; }

		public uint Index { get; set; }

		public ClassJobType ClassJob { get; set; }

		public int ItemLevel { get; set; }

		public uint RewardItemId { get; set; }
	}
}
