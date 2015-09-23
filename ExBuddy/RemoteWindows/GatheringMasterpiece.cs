namespace ExBuddy.RemoteWindows
{
	using ExBuddy.Enums;

	using ff14bot;

	public sealed class GatheringMasterpiece : Window<GatheringMasterpiece>
	{
		public GatheringMasterpiece()
			: base("GatheringMasterpiece") {}

		public static SendActionResult ClickCollect()
		{
			return new GatheringMasterpiece().Collect();
		}

		public SendActionResult Collect()
		{
			return TrySendAction(1, 1, 0);
		}

		public static int Rarity
		{
			get
			{
				return new GatheringMasterpiece().CurrentRarity;
			}
		}

		public int CurrentRarity
		{
			get
			{
				if (IsValid)
				{
					return Core.Memory.Read<int>(Control.Pointer + 0x000001C4);
				}

				return 0;
			}
		}
	}
}