namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using ff14bot;

    [GatheringRotation("Collect470")]
    public class Collect470GatheringRotation : DefaultCollectGatheringRotation
    {
        public override async Task<bool> ExecuteRotation()
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

            if (Core.Player.HasAura((int)AbilityAura.DiscerningEye))
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