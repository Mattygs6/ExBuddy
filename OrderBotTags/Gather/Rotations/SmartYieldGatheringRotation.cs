namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using ff14bot;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("SmartYield", 0, 0)]
    public class SmartYieldGatheringRotation : SmartGatheringRotation, IGetOverridePriority
    {
        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            if (Core.Player.CurrentGP >= 500 && Core.Player.ClassLevel >= 40)
            {
                await tag.Cast(Ability.IncreaseGatherYield2);
                return await base.ExecuteRotation(tag);
            }

            if (Core.Player.CurrentGP >= 400 && Core.Player.ClassLevel >= 30)
            {
                await tag.Cast(Ability.IncreaseGatherYield);
                return await base.ExecuteRotation(tag);
            }

            if (Core.Player.CurrentGP >= 300 && Core.Player.ClassLevel >= 25)
            {
                await tag.Cast(Ability.AdditionalAttempt);
                return await base.ExecuteRotation(tag);
            }

            return true;
        }

        int IGetOverridePriority.GetOverridePriority(GatherCollectableTag tag)
        {
            if (tag.CollectableItem != null)
            {
                return -1;
            }

            if (tag.GatherIncrease == GatherIncrease.Yield
                || (tag.GatherIncrease == GatherIncrease.Auto && Core.Player.ClassLevel >= 40))
            {
                return 9001;
            }

            return -1;
        }
    }
}