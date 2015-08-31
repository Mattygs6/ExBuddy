namespace ExBuddy.OrderBotTags.Gather
{
    using System.Threading.Tasks;

    public interface IGatheringRotation
    {
        GatheringRotationAttribute Attributes { get; }

        bool CanOverride { get; }

        bool ForceGatherIfMissingGpOrTime { get; }

        Task<bool> Prepare(GatherCollectableTag tag);

        Task<bool> ExecuteRotation(GatherCollectableTag tag);

        Task<bool> Gather(GatherCollectableTag tag);

        int ShouldOverrideSelectedGatheringRotation(GatherCollectableTag tag);
    }
}
