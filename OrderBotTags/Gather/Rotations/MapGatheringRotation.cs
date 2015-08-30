namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System;
    using System.Threading.Tasks;

    using ff14bot;

    [GatheringRotation("Map", 0, 8)]
    public class MapGatheringRotation : UnspoiledGatheringRotation
    {
        public override async Task<bool> Prepare(GatherCollectable tag)
        {
            if (Core.Player.HasAura((int)AbilityAura.CollectorsGlove))
            {
                await Actions.Cast(Ability.CollectorsGlove);
            }

            return await base.Prepare(tag);
        }

        public override async Task<bool> ExecuteRotation(GatherCollectable tag)
        {
            return true;
        }

        public override int ShouldOverrideSelectedGatheringRotation(GatherCollectable tag)
        {
            // Only override if the item name ends with ' map'
            if (!tag.GatherItem.ItemData.EnglishName.EndsWith(" map", StringComparison.InvariantCultureIgnoreCase))
            {
                return -1;
            }

            return 100;
        }
    }
}