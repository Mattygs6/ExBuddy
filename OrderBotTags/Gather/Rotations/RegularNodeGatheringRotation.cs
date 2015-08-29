namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System;
    using System.Threading.Tasks;

    using ff14bot;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("RegularNode", 0, 0)]
    public class RegularNodeGatheringRotation : UnspoiledGatheringRotation
    {
        public override async Task<bool> Prepare(GatherCollectable tag)
        {
            if (Core.Player.HasAura((int)AbilityAura.CollectorsGlove))
            {
                await Actions.Cast(Ability.CollectorsGlove);
            }

            return true;
        }

        public override bool ShouldOverrideSelectedGatheringRotation(GatherCollectable tag)
        {
            if (tag.Node.EnglishName.IndexOf("unspoiled", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                return false;
            }

            if (tag.Node.EnglishName.IndexOf("ephemeral", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                return false;
            }

            return true;
        }

        protected override async Task<bool> IncreaseChance(GatherCollectable tag)
        {
            if (Core.Player.CurrentGP >= 50 && tag.GatherItem.Chance < 96)
            {
                await Actions.Cast(Ability.IncreaseGatherChance5);
            }

            if (Core.Player.CurrentGP >= 100 && tag.GatherItem.Chance < 86)
            {
                await Actions.Cast(Ability.IncreaseGatherChance15);
            }

            return true;
        }
    }
}