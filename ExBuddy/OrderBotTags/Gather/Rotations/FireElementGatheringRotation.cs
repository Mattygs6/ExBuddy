namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;
    using Attributes;
    using ff14bot;
    using ff14bot.Managers;

    [GatheringRotation("FireElement", 30, 400)]
    public sealed class FireElementGatheringRotation : SmartGatheringRotation
    {
        public override async Task<bool> ExecuteRotation(ExGatherTag tag)
        {
            if (Core.Player.CurrentGP > 399)
            {
                await Wait();
                Actionmanager.DoAction(234U, Core.Player);
            }

            return await base.ExecuteRotation(tag);
        }
    }
}