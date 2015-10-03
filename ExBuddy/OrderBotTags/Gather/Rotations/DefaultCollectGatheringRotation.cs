namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;

	using ff14bot.Managers;

	[GatheringRotation("DefaultCollect", 600, 30)]
	public sealed class DefaultCollectGatheringRotation : CollectableGatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members

		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			if (tag.IsUnspoiled())
			{
				// We need 5 swings to use this rotation
				if (GatheringManager.SwingsRemaining < 5)
				{
					return -1;
				}
			}

			if (tag.IsEphemeral())
			{
				// We need 4 swings to use this rotation
				if (GatheringManager.SwingsRemaining < 4)
				{
					return -1;
				}
			}

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