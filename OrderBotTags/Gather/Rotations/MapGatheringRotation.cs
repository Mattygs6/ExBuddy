namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System;
    using System.Threading.Tasks;

    using ff14bot;
    using ff14bot.Managers;

    [GatheringRotation("Map", 0, 8)]
    public class MapGatheringRotation : UnspoiledGatheringRotation
    {
        public override async Task<bool> Prepare(GatherCollectableTag tag)
        {
            if (Core.Player.HasAura((int)AbilityAura.CollectorsGlove))
            {
                await tag.Cast(Ability.CollectorsGlove);
            }

            return await base.Prepare(tag);
        }

        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            await IncreaseChance(tag);
            return true;
        }

        public override int ShouldOverrideSelectedGatheringRotation(GatherCollectableTag tag)
        {
            // Only override if the item name ends with ' map'
            if (!tag.GatherItem.ItemData.EnglishName.EndsWith(" map", StringComparison.InvariantCultureIgnoreCase))
            {
                return -1;
            }

            // Only override if we dont' have this map in our inventory
            if (tag.GatherItem.ItemData.ItemCount() > 0)
            {
                return -1;
            }

            return 10000;
        }
    }
}