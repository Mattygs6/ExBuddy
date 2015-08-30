namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System;
    using System.Threading.Tasks;

    using ff14bot;

    [GatheringRotation("OverrideUnspoiled", 600, 25)]
    public class OverrideUnspoiledGatheringRotation : UnspoiledGatheringRotation
    {
        public override async Task<bool> Prepare(GatherCollectableTag tag)
        {
            if (Core.Player.HasAura((int)AbilityAura.CollectorsGlove))
            {
                await tag.Cast(Ability.CollectorsGlove);
            }

            await tag.Cast(Ability.Toil);

            return true;
        }

        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            await tag.Cast(Ability.IncreaseGatherQuality30);

            return true;
        }

        public override int ShouldOverrideSelectedGatheringRotation(GatherCollectableTag tag)
        {
            // Not unspoiled node, don't override
            if (tag.Node.EnglishName.IndexOf("unspoiled", StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                return -1;
            }

            // Only override in free range mode
            if (!tag.FreeRange)
            {
                return -1;
            }

            // Only override if we have 600 or more gp.
            if (Core.Player.CurrentGP < 600)
            {
                return -1;
            }

            // Only override if we get more than 1 item
            if (tag.GatherItem.Amount == 1)
            {
                return -1;
            }

            // We want to be able to get HQ items, this is the purpose.
            if (tag.GatherItem.HqChance <= 0)
            {
                return -1;
            }

            // Only override if we have the default rotation
            if (tag.GatherRotation != "Unspoiled")
            {
                return -1;
            }

            return 5000;
        }
    }
}