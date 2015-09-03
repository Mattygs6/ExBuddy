namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Linq;
    using System.Threading.Tasks;

    using ff14bot;
    using ff14bot.Managers;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("DiscoverUnknowns", 250, 0)]
    public class DiscoverUnknownsGatheringRotation : GatheringRotation, IGetOverridePriority
    {
        public override async Task<bool> Prepare(GatherCollectableTag tag)
        {
            if (tag.IsUnspoiled()
                && Core.Player.CurrentGP >= 550
                && GatheringManager.GatheringWindowItems.Count(i => i.IsUnknown && i.Amount > 0) > 1)
            {

                await tag.Cast(Ability.Toil);
            }
            
            return await base.Prepare(tag);
        }

        int IGetOverridePriority.GetOverridePriority(GatherCollectableTag tag)
        {
            if (tag.GatherItem.IsUnknown)
            {
                return int.MaxValue;
            }

            return -1;
        }
    }
}