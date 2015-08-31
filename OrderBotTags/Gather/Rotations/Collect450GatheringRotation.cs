namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System;
    using System.Threading.Tasks;

    using ff14bot.Managers;

    [GatheringRotation("Collect450", 600, 30)]
    public class Collect450GatheringRotation : DefaultCollectGatheringRotation
    {
        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            await DiscerningMethodical(tag);
            await DiscerningMethodical(tag);
            await SingleMindMethodical(tag);

            await IncreaseChance(tag);

            return true;
        }

        public override int ShouldOverrideSelectedGatheringRotation(GatherCollectableTag tag)
        {
            if (tag.IsUnspoiled())
            {
                // We need 5 swings to use this rotation
                if (GatheringManager.SwingsRemaining < 5)
                {
                    return -1;
                }
            }

            if (tag.IsEphemeral())
            {
                // We need 4 swings to use this rotation
                if (GatheringManager.SwingsRemaining < 4)
                {
                    return -1;
                }
            }

            // if we have a collectable && the collectable value is greater than or equal to 450: Priority 450
            if (tag.CollectableItem != null && tag.CollectableItem.Value >= 450)
            {
                return 450;
            }

            return -1;
        }
    }
}