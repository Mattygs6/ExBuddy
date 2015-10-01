namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System;
	using System.Linq;

	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;

	using ff14bot.Managers;

	[GatheringRotation("Topsoil", 0, 8)]
	public class TopsoilGatheringRotation : GatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members

		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			// Only override if the item name ends with ' topsoil'
			if (!tag.GatherItem.ItemData.EnglishName.EndsWith(" topsoil", StringComparison.InvariantCultureIgnoreCase))
			{
				return -1;
			}

			// Dont' override if we can gather dark matter clusters and they are on our list.
			if (tag.ItemNames.Contains("Dark Matter Cluster", StringComparer.InvariantCultureIgnoreCase)
				&& GatheringManager.GatheringWindowItems.Any(i => i.ItemId == 10335))
			{
				return -1;
			}

			return 10000;
		}

		#endregion
	}
}