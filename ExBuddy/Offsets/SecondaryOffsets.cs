namespace ExBuddy.Offsets
{
    using System;

    using ExBuddy.Attributes;
    using ff14bot;
    public static class FishingOffsets
    {
        [Offset("Search 3B 05 ? ? ? ? 74 D8 Add 2 Read32")]
        public static IntPtr SelectedBaitItemIdPtr;

        public static IntPtr TugTypePtr = Core.Memory.ImageBase + 0x0105DC50;

    }


    public static class GatheringOffsets
    {
        public static IntPtr GatheringBasePtr = Core.Memory.ImageBase + 0x010A43C8;

        public static int Chain = 0x4EC;
        public static int HqChain = 0x4ED;
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
        public static IntPtr BasePtr = Core.Memory.ImageBase + 0x01041164;

        public static int BlueGatherer = 0x10;
        public static int RedCrafter = 0x8;
        public static int RedGatherer = 0x18;
        public static int CenturioSeals = 0x20;
        public static int WeeklyRedCrafter = 0x24;
        public static int WeeklyRedGatherer = 0x28;
    }


    public static class GatheringMasterpieceOffsets
    {
        public static int CurrentRarityOffset = 0x000001C4;
    }

    public static class GuildLeveOffsets
    {
        public static IntPtr AllowancesPtr = Core.Memory.ImageBase + 0x010A4BC4;
    }

    public static class SelectYesNoItemOffsets
    {
        public static IntPtr ItemIdPtr = Core.Memory.ImageBase + 0x0105DC50;
        public static IntPtr CollectabilityValuePtr = Core.Memory.ImageBase + 0x0105DC54;
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