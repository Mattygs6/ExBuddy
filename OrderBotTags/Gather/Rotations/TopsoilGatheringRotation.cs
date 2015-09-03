namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System;
    using System.Linq;

    using ff14bot.Managers;

    [GatheringRotation("Topsoil", 0, 8)]
    public class TopsoilGatheringRotation : GatheringRotation, IGetOverridePriority
    {
        int IGetOverridePriority.GetOverridePriority(GatherCollectableTag tag)
        {
            // Only override if the item name ends with ' topsoil'
            if (!tag.GatherItem.ItemData.EnglishName.EndsWith(" topsoil", StringComparison.InvariantCultureIgnoreCase))
            {
                return -1;
            }

            // Only override if we can't gather dark matter clusters if they are on our list.
            if (!tag.ItemNames.Contains("Dark Matter Cluster", StringComparer.InvariantCultureIgnoreCase)
                || GatheringManager.GatheringWindowItems.All(i => i.ItemId != 10335))
            {
                return -1;
            }

            return 10000;
        }
    }
}