namespace ExBuddy.Offsets
{
	using System;
	using ExBuddy.Attributes;

	public static class RequestOffsets
	{
        [Offset64("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 48 0F BF CF Add 3 TraceRelative")]
        public static IntPtr ItemBasePtr;

        public static int ItemSize = 0x90;

	}

	public static class ScripsOffsets
	{
		// TODO: Real data is a struct with 2 vals, uint ItemId and 4byte val
        [Offset64("Search 48 8D 0D ? ? ? ? 8B D7 E8 ? ? ? ? 03 C6 Add 3 TraceRelative Add 4")]
        public static IntPtr BasePtr;

        public static int BlueGathererOffset = 0x10;
        public static int RedCrafterOffset = 0x8;
        public static int RedGathererOffset = 0x18;
        public static int CenturioSealsOffset = 0x20;
        public static int WeeklyRedCrafterOffset = 0x24;
        public static int WeeklyRedGathererOffset = 0x28;

	}

	//Native functions are used now
	//public static class GatheringMasterpieceOffsets
	//{
	//    [Offset("Search 89 86 ? ? ? ? E8 ? ? ? ? 8B 86 ? ? ? ? 8B 8E ? ? ? ? 6A 00 6A 00 6A 00 6A 00 50 E8 ? ? ? ? F6 47 30 0F Add 2 Read32", true)]
	//    public static int CurrentRarityOffset;
	//}

	public static class GuildLeveOffsets
	{
        [Offset64("Search 88 05 ? ? ? ? 0F B7 41 06 Add 2 TraceRelative")]
        public static IntPtr AllowancesPtr;
	}

	/// <summary>
	///     unused
	/// </summary>
	public static class AetherialReductionOffsets
	{
		public static int CurrentBagSlotOffset = 0x38;

		public static int MaxPurityOffset = 0x40;

		public static int PurityOffset = 0x3C;
	}

	/// <summary>
	///     unused
	/// </summary>
	public static class DesynthesisOffsets
	{
		public static int CurrentBagSlotOffset = 0x2D0;
	}
}