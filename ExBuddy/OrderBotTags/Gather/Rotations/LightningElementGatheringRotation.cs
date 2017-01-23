namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;
    using Attributes;
    using ff14bot;
    using ff14bot.Managers;

    [GatheringRotation("LightningElement", 30, 400)]
    public sealed class LightningElementGatheringRotation : SmartGatheringRotation
    {
        public override async Task<bool> ExecuteRotation(ExGatherTag tag)
        {
            if (Core.Player.CurrentGP > 399)
            {
                await Wait();
                Actionmanager.DoAction(219U, Core.Player);
            }

            return await base.ExecuteRotation(tag);
        }
    }
}