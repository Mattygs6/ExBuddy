namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;

	[GatheringRotation("DefaultCollect", 30, 600)]
	public sealed class DefaultCollectGatheringRotation : CollectableGatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members

		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			// if we have a collectable Priority 510
			if (tag.CollectableItem != null && tag.CollectableItem.Value >= 510)
			{
				return 510;
			}

			return -1;
		}

		#endregion
	}
}