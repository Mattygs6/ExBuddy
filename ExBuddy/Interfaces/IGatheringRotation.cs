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

		Task<bool> Prepare(ExGatherTag tag);

		Task<bool> ExecuteRotation(ExGatherTag tag);

		Task<bool> Gather(ExGatherTag tag);

		int ResolveOverridePriority(ExGatherTag tag);
	}
}