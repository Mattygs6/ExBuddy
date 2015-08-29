namespace ExBuddy.OrderBotTags.Gather
{
    using System.Threading.Tasks;

    public interface IGatheringRotation
    {
        bool ForceGatherIfMissingGpOrTime { get; }

        Task<bool> Prepare(GatherCollectable tag);

        Task<bool> ExecuteRotation(GatherCollectable tag);

        Task<bool> Gather(GatherCollectable tag);

        bool ShouldOverrideSelectedGatheringRotation(GatherCollectable tag);
    }
}
