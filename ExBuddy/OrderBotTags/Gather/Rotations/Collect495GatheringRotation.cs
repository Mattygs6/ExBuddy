namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Threading.Tasks;

	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;

	using ff14bot.Managers;

	[GatheringRotation("Collect495", 600, 28)]
	public sealed class Collect495GatheringRotation : CollectableGatheringRotation, IGetOverridePriority
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

			// if we have a collectable && the collectable value is greater than or equal to 495: Priority 495
			if (tag.CollectableItem != null && tag.CollectableItem.Value >= 495)
			{
				return 495;
			}

			return -1;
		}

		#endregion

		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			await DiscerningMethodical(tag);
			await Discerning(tag);
			await AppraiseAndRebuff(tag);
			await Methodical(tag);

			await IncreaseChance(tag);

			return true;
		}
	}
}