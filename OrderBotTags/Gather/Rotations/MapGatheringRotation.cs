namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System;
    using System.Threading.Tasks;

    [GatheringRotation("Map", 0, 25)]
    public class MapGatheringRotation : DefaultGatheringRotation
    {
        public override async Task<bool> ExecuteRotation(GatherCollectable tag)
        {
            return true;
        }

        public override bool ShouldOverrideSelectedGatheringRotation(GatherCollectable tag)
        {
            // Only override if the item name ends with ' map'
            if (!tag.GatherItem.ItemData.EnglishName.EndsWith(" map", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}