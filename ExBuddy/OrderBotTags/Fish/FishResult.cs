namespace ExBuddy.OrderBotTags.Fish
{
	using System;
	using ExBuddy.Enumerations;

	public class FishResult
	{
		public string FishName
		{
			get
			{
				if (IsHighQuality)
				{
					return Name.Substring(0, Name.Length - 2);
				}

				return Name;
			}
		}

		public bool IsHighQuality { get; set; }

		public string Name { get; set; }

		public float Size { get; set; }

		public bool IsKeeper(Keeper keeper)
		{
			if (!string.Equals(keeper.Name, FishName, StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}

			if ((!keeper.Action.HasFlag(KeeperAction.KeepHq) && IsHighQuality))
			{
				return false;
			}

			if ((!keeper.Action.HasFlag(KeeperAction.KeepNq) && !IsHighQuality))
			{
				return false;
			}

			return true;
		}

		public bool ShouldMooch(Keeper keeper)
		{
			if (!keeper.Action.HasFlag((KeeperAction) 0x04))
			{
				return false;
			}

			return true;
		}
	}
}