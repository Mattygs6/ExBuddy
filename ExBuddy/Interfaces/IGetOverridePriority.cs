namespace ExBuddy.Interfaces
{
	using ExBuddy.OrderBotTags.Gather;

	public interface IGetOverridePriority
	{
		int GetOverridePriority(GatherCollectableTag tag);
	}
}