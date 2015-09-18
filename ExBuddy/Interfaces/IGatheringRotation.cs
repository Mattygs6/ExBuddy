namespace ExBuddy.Interfaces
{
    using System.Threading.Tasks;

    using ExBuddy.Attributes;
    using ExBuddy.OrderBotTags.Gather;

    public interface IGatheringRotation
    {
        GatheringRotationAttribute Attributes { get; }

        bool CanBeOverriden { get; }

        bool ShouldForceGather { get; }

        Task<bool> Prepare(GatherCollectableTag tag);

        Task<bool> ExecuteRotation(GatherCollectableTag tag);

        Task<bool> Gather(GatherCollectableTag tag);

        int ResolveOverridePriority(GatherCollectableTag tag);
    }
}
