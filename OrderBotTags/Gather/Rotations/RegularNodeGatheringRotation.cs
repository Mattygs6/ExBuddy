namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System;
    using System.Threading.Tasks;

    using ff14bot;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("RegularNode", 0, 0)]
    public class RegularNodeGatheringRotation : UnspoiledGatheringRotation
    {
        public override async Task<bool> Prepare(GatherCollectableTag tag)
        {
            if (Core.Player.HasAura((int)AbilityAura.CollectorsGlove))
            {
                await tag.Cast(Ability.CollectorsGlove);
            }

            return true;
        }

        public override int ShouldOverrideSelectedGatheringRotation(GatherCollectableTag tag)
        {
            if (tag.Node.EnglishName.IndexOf("unspoiled", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                return -1;
            }

            if (tag.Node.EnglishName.IndexOf("ephemeral", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                return -1;
            }

            return 8000;
        }

        protected override async Task<bool> IncreaseChance(GatherCollectableTag tag)
        {
            if (Core.Player.CurrentGP >= 50 && tag.GatherItem.Chance < 96)
            {
                await tag.Cast(Ability.IncreaseGatherChance5);
            }

            if (Core.Player.CurrentGP >= 100 && tag.GatherItem.Chance < 86)
            {
                await tag.Cast(Ability.IncreaseGatherChance15);
            }

            return true;
        }
    }
}