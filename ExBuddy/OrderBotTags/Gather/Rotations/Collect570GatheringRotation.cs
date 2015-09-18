namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using ExBuddy.Attributes;
    using ExBuddy.Interfaces;

    using ff14bot.Managers;

    [GatheringRotation("Collect570", 600, 34)]
    public sealed class Collect570GatheringRotation : CollectableGatheringRotation, IGetOverridePriority
    {
        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            await DiscerningUtmostMethodical(tag);
            await DiscerningUtmostMethodical(tag);
            await Methodical(tag);
            await Methodical(tag);

            await IncreaseChance(tag);

            return true;
        }

        int IGetOverridePriority.GetOverridePriority(GatherCollectableTag tag)
        {
            // We need 5 swings to use this rotation
            if (GatheringManager.SwingsRemaining < 5)
            {
                return -1;
            }

            // if we have a collectable && the collectable value is greater than or equal to 570: Priority 570
            if (tag.CollectableItem != null && tag.CollectableItem.Value >= 570)
            {
                return 570;
            }

            return -1;
        }
    }
}