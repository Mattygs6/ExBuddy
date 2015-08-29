namespace ExBuddy.OrderBotTags.Gather
{
    using System.Threading.Tasks;

    using ff14bot.Managers;

    public interface IGatheringRotation
    {
        Task<GatheringItem> Prepare(uint slot);
        Task<bool> ExecuteRotation(GatheringItem gatherItem);
        Task<bool> Gather(uint slot);
    }
}
