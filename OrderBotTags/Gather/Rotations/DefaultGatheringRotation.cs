namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using ff14bot;
    using ff14bot.Managers;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("Default", 0, 23)]
    public class DefaultGatheringRotation : IGatheringRotation
    {
        public virtual async Task<GatheringItem> Prepare(uint slot)
        {
            var hits = 0;
            GatheringItem item = null;
            while (GatheringManager.WindowOpen && hits < 1)
            {
                await
                    Coroutine.Wait(
                        5000,
                        () => (item = GatheringManager.GetGatheringItemByIndex(slot)) != null);
                if (item != null)
                {
                    item.GatherItem();
                    hits++;
                    await Coroutine.Sleep(2200);
                }
            }

            return item;
        }

        public virtual async Task<bool> ExecuteRotation(GatheringItem gatherItem)
        {
            if (Core.Player.CurrentGP >= 500)
            {
                await Actions.Cast(Ability.IncreaseGatherYield2);
            }
            if (Core.Player.CurrentGP >= 50)
            {
                await Actions.Cast(Ability.IncreaseGatherChance5);
            }

            return true;
        }

        public virtual async Task<bool> Gather(uint slot)
        {
            while (GatheringManager.SwingsRemaining > 0)
            {
                await Coroutine.Sleep(500);
                GatheringManager.GetGatheringItemByIndex(5).GatherItem();
            }

            await Coroutine.Sleep(1000);

            return true;
        }
    }
}