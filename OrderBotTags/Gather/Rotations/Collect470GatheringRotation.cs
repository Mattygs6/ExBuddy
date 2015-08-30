namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System;
    using System.Threading.Tasks;

    using ff14bot.Managers;

    [GatheringRotation("Collect470")]
    public class Collect470GatheringRotation : DefaultCollectGatheringRotation
    {
        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            await tag.Cast(Ability.DiscerningEye);

            await AppraiseAndRebuff(tag);
            await AppraiseAndRebuff(tag);

            await Methodical(tag);

            await IncreaseChance(tag);

            return true;
        }

        public override int ShouldOverrideSelectedGatheringRotation(GatherCollectableTag tag)
        {
            if (tag.Node.EnglishName.IndexOf("unspoiled", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                // We need 5 swings to use this rotation
                if (GatheringManager.SwingsRemaining < 5)
                {
                    return -1;
                }
            }

            if (tag.Node.EnglishName.IndexOf("ephemeral", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                // We need 4 swings to use this rotation
                if (GatheringManager.SwingsRemaining < 4)
                {
                    return -1;
                }
            }

            // if we have a collectable && the collectable value is greater than or equal to 470: Priority 470
            if (tag.CollectableItem != null && tag.CollectableItem.Value >= 470)
            {
                return 470;
            }

            return -1;
        }

        private async Task AppraiseAndRebuff(GatherCollectableTag tag)
        {
            await Impulsive(tag);

            if (HasDiscerningEye)
            {
                await tag.Cast(Ability.SingleMind);
            }
            else
            {
                await tag.Cast(Ability.DiscerningEye);
            }
        }
    }
}