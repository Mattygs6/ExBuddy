namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using ff14bot;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("DiscoverUnknowns", 250, 0)]
    public class DiscoverUnknownsGatheringRotation : RegularNodeGatheringRotation
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
            if (tag.GatherItem.IsUnknown)
            {
                return int.MaxValue;
            }

            return -1;
        }
    }
}