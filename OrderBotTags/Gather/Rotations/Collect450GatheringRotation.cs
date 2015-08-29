namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using ff14bot;
    using ff14bot.Managers;

    [GatheringRotation("Collect450", 600, 30)]
    public class Collect450GatheringRotation : DefaultCollectGatheringRotation
    {
        public override async Task<bool> ExecuteRotation(GatheringItem gatherItem)
        {
            await DiscerningMethodical();
            await DiscerningMethodical();
            await SingleMindMethodical();

            if (Core.Player.CurrentGP >= 50)
            {
                await Actions.Cast(Ability.IncreaseGatherChance5);
            }

            return true;
        }
    }
}