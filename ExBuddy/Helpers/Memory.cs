namespace ExBuddy.Helpers
{
	using System;
	using System.Linq;

	using ExBuddy.OrderBotTags.Behaviors.Objects;

	using ff14bot;

	public static class Memory
	{
		public static class Request
		{
			public static uint ItemId1
			{
				get
				{
					return Core.Memory.NoCacheRead<uint>(Core.Memory.ImageBase + 0x0103FD7C);
				}
			}

			public static uint ItemId2
			{
				get
				{
					return Core.Memory.NoCacheRead<uint>(Core.Memory.ImageBase + 0x0103FDF4);
				}
			}

			public static uint ItemId3
			{
				get
				{
					return Core.Memory.NoCacheRead<uint>(Core.Memory.ImageBase + 0x0103FE6C);
				}
			}

			public static uint[] ItemsToTurnIn
			{
				get
				{
					return new[] { ItemId1, ItemId2, ItemId3 }.Where(i => i > 0).ToArray();
				}
			}
		}

		public static class Scrips
		{
			public static readonly IntPtr BasePointer = IntPtr.Add(Core.Memory.ImageBase, 0x010379AC);

			public static int BlueCrafter
			{
				get
				{
					return Core.Memory.NoCacheRead<int>(BasePointer);
				}
			}

			public static int BlueGatherer
			{
				get
				{
					return Core.Memory.NoCacheRead<int>(BasePointer + 16);
				}
			}

			public static int RedCrafter
			{
				get
				{
					return Core.Memory.NoCacheRead<int>(BasePointer + 8);
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
}