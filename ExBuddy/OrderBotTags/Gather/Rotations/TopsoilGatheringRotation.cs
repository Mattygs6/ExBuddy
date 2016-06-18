namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System;
	using System.Linq;
	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;
	using ff14bot.Managers;

	[GatheringRotation("Topsoil", 8)]
	public class TopsoilGatheringRotation : GatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members

		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			if (tag.GatherItem == null)
			{
				return -1;
			}

			// Only override if the item name ends with ' topsoil'
			if (!tag.GatherItem.ItemData.EnglishName.EndsWith(" topsoil", StringComparison.InvariantCultureIgnoreCase))
			{
				return -1;
			}

			// Dont' override if we can gather dark matter clusters and they are on our list.
			if (tag.Items.Any(i => i.Name.Equals("Dark Matter Cluster", StringComparison.InvariantCultureIgnoreCase))
			    && GatheringManager.GatheringWindowItems.Any(i => i.ItemId == 10335))
			{
				return -1;
			}

			return 10000;
		}

		#endregion
	}
}