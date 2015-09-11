namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using ff14bot;
    using ff14bot.Managers;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("Unspoiled", 500, 23)]
    public sealed class UnspoiledGatheringRotation : GatheringRotation, IGetOverridePriority
    {
        public override async Task<bool> Prepare(GatherCollectableTag tag)
        {
            await Wait();

            return tag.GatherItem.TryGatherItem() && await base.Prepare(tag);
        }

        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            if (Core.Player.CurrentGP >= 500)
            {
                await tag.Cast(Ability.IncreaseGatherYield2);
            }

            return await base.ExecuteRotation(tag);
        }

        protected override async Task<bool> IncreaseChance(GatherCollectableTag tag)
        {
            if (Core.Player.CurrentGP >= 50 && tag.GatherItem.Chance < 100)
            {
                if (Core.Player.ClassLevel >= 23 && GatheringManager.SwingsRemaining == 1)
                {
                    return await tag.Cast(Ability.IncreaseGatherChanceOnce15);
                }

                return await tag.Cast(Ability.IncreaseGatherChance5);
            }

            return false;
        }

        int IGetOverridePriority.GetOverridePriority(GatherCollectableTag tag)
        {
            if (tag.IsUnspoiled() && tag.CollectableItem == null)
            {
                return 8000;
            }

            return -1;
        }
    }
}