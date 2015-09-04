namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using ff14bot;
    using ff14bot.Managers;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("SmartYield", 0, 0)]
    public class SmartYieldGatheringRotation : SmartGatheringRotation, IGetOverridePriority
    {
        private int maxGpNodeCount;

        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            var level = Core.Player.ClassLevel;
            // Staring count at -30 since we regain that within the next node and we usually have +30 above the threshold on our gear at each point.
            if (Core.Player.CurrentGP >= Core.Player.MaxGP - 30)
            {
                maxGpNodeCount++;
            }

            if (GatheringManager.SwingsRemaining > 4 ||
                (level < 50 && maxGpNodeCount > 4) || maxGpNodeCount > 6)
            {
                maxGpNodeCount = 0;

                if (Core.Player.CurrentGP >= 500 && level >= 40)
                {
                    await tag.Cast(Ability.IncreaseGatherYield2);
                    return await base.ExecuteRotation(tag);
                }

                if (Core.Player.CurrentGP >= 400 && level >= 30 && level < 40)
                {
                    await tag.Cast(Ability.IncreaseGatherYield);
                    return await base.ExecuteRotation(tag);
                }

                if (Core.Player.CurrentGP >= 300 && level >= 25 && level < 30)
                {
                    while (GatheringManager.ShouldPause(DataManager.SpellCache[(uint)Ability.Preparation]))
                    {
                        await Coroutine.Yield();
                    }

                    tag.GatherItem.GatherItem();

                    await tag.Cast(Ability.AdditionalAttempt);
                    return await base.ExecuteRotation(tag);
                }
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