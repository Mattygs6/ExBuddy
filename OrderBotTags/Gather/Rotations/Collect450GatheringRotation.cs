namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using ff14bot;

    [GatheringRotation("Collect450", 600, 30)]
    public class Collect450GatheringRotation : DefaultCollectGatheringRotation
    {
        public override async Task<bool> ExecuteRotation()
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