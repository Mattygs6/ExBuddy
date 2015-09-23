namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Threading.Tasks;

	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;

	using ff14bot.Managers;

	[GatheringRotation("Collect345", 0, 24)]
	public sealed class Collect345GatheringRotation : CollectableGatheringRotation, IGetOverridePriority
	{
		public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
		{
			await Methodical(tag);
			await Methodical(tag);
			await Methodical(tag);

			await IncreaseChance(tag);

			return true;
		}

		int IGetOverridePriority.GetOverridePriority(GatherCollectableTag tag)
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

			// if we have a collectable && the collectable value is greater than or equal to 345: Priority 345
			if (tag.CollectableItem != null && tag.CollectableItem.Value >= 345)
			{
				return 345;
			}

			return -1;
		}
	}
}