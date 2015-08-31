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

        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            await IncreaseChance(tag);

            return true;
        }

        public override int ShouldOverrideSelectedGatheringRotation(GatherCollectableTag tag)
        {
            if (tag.IsEphemeral() || tag.IsUnspoiled())
            {
                return -1;
            }

            return 8000;
        }

        protected override async Task<bool> IncreaseChance(GatherCollectableTag tag)
        {
            if (Core.Player.CurrentGP >= 250 && tag.GatherItem.Chance < 51)
            {
                return await tag.Cast(Ability.IncreaseGatherChance50);
            }

            if (Core.Player.CurrentGP >= 100 && tag.GatherItem.Chance < 86)
            {
                return await tag.Cast(Ability.IncreaseGatherChance15);
            }

            if (Core.Player.CurrentGP >= 50 && tag.GatherItem.Chance < 96)
            {
                return await tag.Cast(Ability.IncreaseGatherChance5);
            }

            return true;
        }
    }
}