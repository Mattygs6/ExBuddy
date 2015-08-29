namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using ff14bot;
    using ff14bot.Managers;

    [GatheringRotation("Collect470")]
    public class Collect470GatheringRotation : DefaultCollectGatheringRotation
    {
        public override async Task<bool> ExecuteRotation(GatheringItem gatherItem)
        {
            await Actions.Cast(Ability.DiscerningEye);

            await AppraiseAndRebuff();
            await AppraiseAndRebuff();

            await Actions.Cast(Ability.MethodicalAppraisal);

            if (Core.Player.CurrentGP >= 50)
            {
                await Actions.Cast(Ability.IncreaseGatherChance5);
            }

            return true;
        }

        private async Task AppraiseAndRebuff()
        {
            await Actions.Cast(Ability.ImpulsiveAppraisal);

            if (HasDiscerningEye)
            {
                await Actions.Cast(Ability.SingleMind);
            }
            else
            {
                await Actions.Cast(Ability.DiscerningEye);
            }
        }
    }
}