namespace ExBuddy.Offsets
{
    using System;

    using ExBuddy.Attributes;
    using ff14bot;
    public static class FishingOffsets
    {
        [Offset("Search 3B 05 ? ? ? ? 74 D8 Add 2 Read32")]
        public static IntPtr SelectedBaitItemIdPtr;

        [Offset("Search 68 ? ? ? ? 50 8B 82 Add 1 Read32")]
        public static IntPtr TugTypePtr;

    }


    public static class GatheringOffsets
    {
        [Offset("Search 68 ? ? ? ? E8 ? ? ? ? 8B 85 ? ? ? ? 83 C4 0C 56 Add 1 Read32")]
        public static IntPtr GatheringBasePtr;

        [Offset("Search 0F B6 80 ? ? ? ? 8B 4D FC 66 3B 41 14 Add 3 Read32", true)]
        public static int ChainOffset;

        [Offset("Search 0F B6 90 ? ? ? ? 8B 45 FC Add 3 Read32", true)]
        public static int HqChainOffset;
    }

    public static class RequestOffsets
    {
        public static IntPtr ItemBasePtr = Core.Memory.ImageBase + 0x010491B0;
        public static int Item2Offset = 0x78;
        public static int Item3Offset = 0xF0;
    }

    public static class ScripsOffsets
    {
        // TODO: Real data is a struct with 2 vals, uint ItemId and 4byte val
        [Offset("Search 89 0C C5 ? ? ? ? 5F Add 3 Read32")]
        public static IntPtr BasePtr;

        public static int BlueGathererOffset = 0x10;
        public static int RedCrafterOffset = 0x8;
        public static int RedGathererOffset = 0x18;
        public static int CenturioSealsOffset = 0x20;
        public static int WeeklyRedCrafterOffset = 0x24;
        public static int WeeklyRedGathererOffset = 0x28;
    }


    public static class GatheringMasterpieceOffsets
    {
        [Offset("Search 89 86 ? ? ? ? E8 ? ? ? ? 8B 86 ? ? ? ? 8B 8E ? ? ? ? 6A 00 6A 00 6A 00 6A 00 50 E8 ? ? ? ? F6 47 30 0F Add 2 Read32",true)]
        public static int CurrentRarityOffset;
    }

    public static class GuildLeveOffsets
    {
        [Offset("Search 88 15 ? ? ? ? 66 8B 48 06 Add 2 Read32")]
        public static IntPtr AllowancesPtr;
    }

    public static class SelectYesNoItemOffsets
    {
        [Offset("Search 68 ? ? ? ? 50 8B 82 Add 1 Read32")]
        public static IntPtr ItemIdPtr;

        [Offset("Search 8B 4F ?? 8B 86 ?? ?? ?? ?? 89 88 Add 2 Read8")]
        public static int CollectabilityValueOffset;
    }


    public static class AetherialReductionOffsets
    {
        public static int CurrentBagSlotOffset = 0x38;
        public static int PurityOffset = 0x3C;
        public static int MaxPurityOffset = 0x40;
    }

    public static class DesynthesisOffsets
    {
        public static int CurrentBagSlotOffset = 0x2D0;
    }
}