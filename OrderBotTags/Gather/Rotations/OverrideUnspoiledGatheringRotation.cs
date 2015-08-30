namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using ff14bot;

    [GatheringRotation("OverrideUnspoiled", 600, 25)]
    public class OverrideUnspoiledGatheringRotation : UnspoiledGatheringRotation
    {
        public override async Task<bool> Prepare(GatherCollectable tag)
        {
            if (Core.Player.HasAura((int)AbilityAura.CollectorsGlove))
            {
                await Actions.Cast(Ability.CollectorsGlove);
            }

            await Actions.Cast(Ability.Toil);

            return true;
        }

        public override async Task<bool> ExecuteRotation(GatherCollectable tag)
        {
            await Actions.Cast(Ability.IncreaseGatherQuality30);

            return true;
        }

        public override int ShouldOverrideSelectedGatheringRotation(GatherCollectable tag)
        {
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
            if (tag.GatherItem.HqChance == 0)
            {
                return -1;
            }

            // Only override if we have the default rotation
            if (tag.GatherRotation != "Unspoiled")
            {
                return -1;
            }

            return 1000;
        }
    }
}