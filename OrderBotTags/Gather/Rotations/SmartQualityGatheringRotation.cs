namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using ff14bot;
    using ff14bot.Managers;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("SmartQuality", 0, 0)]
    public class SmartQualityGatheringRotation : SmartGatheringRotation, IGetOverridePriority
    {
        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            if (Core.Player.CurrentGP >= 300 && GatheringManager.SwingsRemaining > 4)
            {
                await tag.Cast(Ability.IncreaseGatherQuality30);
                await base.ExecuteRotation(tag);

                if (tag.GatherItem.Chance == 100 && Core.Player.CurrentGP >= 300)
                {
                    while (GatheringManager.ShouldPause(DataManager.SpellCache[(uint)Ability.Preparation]))
                    {
                        await Coroutine.Yield();
                    }

                    tag.GatherItem.GatherItem();

                    await tag.Cast(Ability.AdditionalAttempt);
                }

                return true;
            }

            // Approx 30 gp or more between running to nodes, we are basically capped here so just use 100 gp
            if (Core.Player.CurrentGP >= Core.Player.MaxGP - 30)
            {
                await tag.Cast(Ability.IncreaseGatherQuality10);
                return true;
            }

            return true;
        }

        int IGetOverridePriority.GetOverridePriority(GatherCollectableTag tag)
        {
            if (tag.CollectableItem != null)
            {
                return -1;
            }

            if (tag.GatherItem.HqChance < 1)
            {
                return -1;
            }

            if (tag.GatherIncrease == GatherIncrease.Quality 
                || (tag.GatherIncrease == GatherIncrease.Auto && Core.Player.ClassLevel >= 15 && Core.Player.ClassLevel < 40))
            {
                return 9001;
            }

            return -1;
        }
    }
}