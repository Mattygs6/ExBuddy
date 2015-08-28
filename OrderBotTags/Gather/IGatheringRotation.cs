namespace ExBuddy.OrderBotTags.Gather
{
    using System.Threading.Tasks;

    public interface IGatheringRotation
    {
        Task<bool> Prepare(uint slot);
        Task<bool> ExecuteRotation();
        Task<bool> Gather(uint slot);
    }
}
