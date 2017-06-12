namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;
    using Attributes;
    using ff14bot;
    using ff14bot.Managers;

    [GatheringRotation("WindElement", 30, 400)]
    public sealed class WindElementGatheringRotation : SmartGatheringRotation
    {
        public override async Task<bool> ExecuteRotation(ExGatherTag tag)
        {
            if (Core.Player.CurrentGP > 399)
            {
                await Wait();
                Actionmanager.DoAction(292U, Core.Player);
            }

            return await base.ExecuteRotation(tag);
        }
    }
}