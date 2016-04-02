namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System;
	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;
	using ff14bot.Managers;

	[GatheringRotation("Map", 8)]
	public class MapGatheringRotation : GatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members

		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			if (tag.GatherItem == null)
			{
				return -1;
			}

			// Only override if the item name ends with ' map'
			if (!tag.GatherItem.ItemData.EnglishName.EndsWith(" map", StringComparison.InvariantCultureIgnoreCase))
			{
				return -1;
			}

			// Only override if we dont' have this map in our inventory
			if (tag.GatherItem.ItemData.ItemCount() > 0)
			{
				return -1;
			}

			return 10000;
		}

		#endregion
	}
}