namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Linq;
    using System.Threading.Tasks;

    using ExBuddy.OrderBotTags.Common;

    using ff14bot;
    using ff14bot.Managers;

    // TODO: if can peek, then we need to allow it to redo beforegather logic
    //Name, RequiredGp, RequiredTime
    [GatheringRotation("DiscoverUnknowns", 250, 0)]
    public class DiscoverUnknownsGatheringRotation : GatheringRotation, IGetOverridePriority
    {
        public override async Task<bool> Prepare(GatherCollectableTag tag)
        {
            if (tag.IsUnspoiled()
                && Core.Player.CurrentGP >= 550
                && GatheringManager.GatheringWindowItems.Count(i => i.Chance == 25 && i.Amount > 0) > 1)
            {

                await tag.Cast(Ability.Toil);
            }
            
            return await base.Prepare(tag);
        }

        int IGetOverridePriority.GetOverridePriority(GatherCollectableTag tag)
        {
            if (tag.GatherItem.IsUnknown || (tag.IsUnspoiled() && tag.GatherItem.Chance == 25))
            {
                return int.MaxValue;
            }

            return -1;
        }
    }
}