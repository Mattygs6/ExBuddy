namespace ExBuddy.Helpers
{
    using System;

    using ExBuddy.OrderBotTags.Behaviors.Objects;

    using ff14bot;

    public static class Scrips
    {
        public static readonly IntPtr BasePointer = IntPtr.Add(Core.Memory.Process.MainModule.BaseAddress, 0x010379AC);

        public static int BlueCrafter
        {
            get
            {
                return Core.Memory.NoCacheRead<int>(BasePointer);
            }
        }

        public static int RedCrafter
        {
            get
            {
                return Core.Memory.NoCacheRead<int>(BasePointer + 8);
            }
        }

        public static int BlueGatherer
        {
            get
            {
                return Core.Memory.NoCacheRead<int>(BasePointer + 16);
            }
        }

        public static int RedGatherer
        {
            get
            {
                return Core.Memory.NoCacheRead<int>(BasePointer + 24);
            }
        }

        public static int GetRemainingScripsByShopType(ShopType shopType)
        {
            switch (shopType)
            {
                case ShopType.BlueCrafter:
                    return BlueCrafter;
                case ShopType.RedCrafter:
                    return RedCrafter;
                case ShopType.BlueGatherer:
                    return BlueGatherer;
                case ShopType.RedGatherer:
                    return RedGatherer;
            }

            return 0;
        }
    }
}