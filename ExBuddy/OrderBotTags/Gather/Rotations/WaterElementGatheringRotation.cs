namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;
    using Attributes;
    using ff14bot;
    using ff14bot.Managers;

    [GatheringRotation("WaterElement", 30, 400)]
    public sealed class WaterElementGatheringRotation : SmartGatheringRotation
    {
        public override async Task<bool> ExecuteRotation(ExGatherTag tag)
        {
            if (Core.Player.CurrentGP > 399)
            {
                await Wait();
                Actionmanager.DoAction(293U, Core.Player);
            }
            
            return await base.ExecuteRotation(tag);
        }
    }
}