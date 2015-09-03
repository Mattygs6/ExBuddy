namespace ExBuddy.OrderBotTags.Gather
{
    using System.Threading.Tasks;

    public interface IGetOverridePriority
    {
        int GetOverridePriority(GatherCollectableTag tag);
    }

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
