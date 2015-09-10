namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;
    using System.Windows.Media;

    using Buddy.Coroutines;

    using ff14bot;
    using ff14bot.Helpers;
    using ff14bot.Managers;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("SmartYield", 0, 0)]
    public class SmartYieldGatheringRotation : SmartGatheringRotation, IGetOverridePriority
    {
        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            var level = Core.Player.ClassLevel;

            if (GatheringManager.SwingsRemaining > 4 ||
                ShouldForceUseRotation(tag, level))
            {
                if (Core.Player.CurrentGP >= 500 && level >= 40)
                {
                    await tag.Cast(Ability.IncreaseGatherYield2);
                    return await base.ExecuteRotation(tag);
                }

                if (Core.Player.CurrentGP >= 400 && level >= 30 && (level < 40 || Core.Player.MaxGP < 500))
                {
                    await tag.Cast(Ability.IncreaseGatherYield);
                    return await base.ExecuteRotation(tag);
                }

                if (Core.Player.CurrentGP >= 300 && level >= 25 && (level < 30 || Core.Player.MaxGP < 400))
                {
                    while (GatheringManager.ShouldPause(DataManager.SpellCache[(uint)Ability.Preparation]))
                    {
                        await Coroutine.Yield();
                    }

                    if (!tag.GatherItem.TryGatherItem())
                    {
                        return false;
                    }

                    await tag.Cast(Ability.AdditionalAttempt);
                    return await base.ExecuteRotation(tag);
                }
            }

            return true;
        }

        private bool ShouldForceUseRotation(GatherCollectableTag tag, uint level)
        {
            if ((level < 50 && tag.NodesGatheredAtMaxGp > 4) || tag.NodesGatheredAtMaxGp > 6)
            {
                Logging.Write(
                    Colors.Chartreuse,
                    "GatherCollectable: Using Gp since we have gathered {0} nodes at max Gp.",
                    tag.NodesGatheredAtMaxGp);

                return true;
            }

            return false;
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