namespace ExBuddy.Helpers
{
	using System;
	using System.Linq;

	using ExBuddy.OrderBotTags.Behaviors.Objects;

	using ff14bot;

	public static class Memory
	{
		public static class Bait
		{
			public static uint SelectedBaitItemId
			{
				get
				{
					return Core.Memory.Read<uint>(Core.Memory.ImageBase + 0x01042828);
				}
			}
		}

		public static class Gathering
		{
			public static byte Chain
			{
				get
				{
					return Core.Memory.Read<byte>(Core.Memory.ImageBase + 0x010A43C8 + 1260);
				}
			}

			public static byte HqChain
			{
				get
				{
					return Core.Memory.Read<byte>(Core.Memory.ImageBase + 0x010A43C8 + 1261);
				}
			}
		}

		public static class Request
		{
			public static uint ItemId1
			{
				get
				{
					return Core.Memory.NoCacheRead<uint>(Core.Memory.ImageBase + 0x010491B0);
				}
			}

			public static uint ItemId2
			{
				get
				{
					return Core.Memory.NoCacheRead<uint>(Core.Memory.ImageBase + 0x010491B0 + 120);
				}
			}

			public static uint ItemId3
			{
				get
				{
					return Core.Memory.NoCacheRead<uint>(Core.Memory.ImageBase + 0x010491B0 + 240);
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
			// TODO: Real data is a struct with 2 vals, uint ItemId and 4byte val
			public static readonly IntPtr BasePointer = IntPtr.Add(Core.Memory.ImageBase, 0x01041164);

			public static int BlueCrafter
			{
				get
				{
					return Core.Memory.Read<int>(BasePointer);
				}
			}

			public static int BlueGatherer
			{
				get
				{
					return Core.Memory.Read<int>(BasePointer + 16);
				}
			}

			public static int RedCrafter
			{
				get
				{
					return Core.Memory.Read<int>(BasePointer + 8);
				}
			}

			public static int RedGatherer
			{
				get
				{
					return Core.Memory.Read<int>(BasePointer + 24);
				}
			}

			public static int CenturioSeals
			{
				get
				{
					return Core.Memory.Read<int>(BasePointer + 32);
				}
			}

			public static int WeeklyRedCrafter
			{
				get
				{
					return Core.Memory.Read<int>(BasePointer + 36);
				}
			}

			public static int WeeklyRedGatherer
			{
				get
				{
					return Core.Memory.Read<int>(BasePointer + 40);
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