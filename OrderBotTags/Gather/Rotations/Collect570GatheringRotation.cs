namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using ff14bot;
    using ff14bot.Managers;

    [GatheringRotation("Collect570", 600, 34)]
    public class Collect570GatheringRotation : DefaultCollectGatheringRotation
    {
        public override async Task<bool> ExecuteRotation(GatheringItem gatherItem)
        {
            await Actions.Cast(Ability.UtmostCaution);
            await Actions.Cast(Ability.MethodicalAppraisal);
            
            await Actions.Cast(Ability.UtmostCaution);
            await Actions.Cast(Ability.MethodicalAppraisal);

            await DiscerningMethodical();
            await DiscerningMethodical();

            if (Core.Player.CurrentGP >= 50)
            {
                await Actions.Cast(Ability.IncreaseGatherChance5);
            }

            return true;
        }
    }
}