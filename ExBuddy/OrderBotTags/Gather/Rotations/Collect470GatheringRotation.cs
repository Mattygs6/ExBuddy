namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Threading.Tasks;

	using ExBuddy.Attributes;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;

	using ff14bot.Managers;

	[GatheringRotation("Collect470", 600, 30)]
	public sealed class Collect470GatheringRotation : CollectableGatheringRotation, IGetOverridePriority
	{
		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			await tag.Cast(Ability.DiscerningEye);

			await AppraiseAndRebuff(tag);
			await AppraiseAndRebuff(tag);

			await Methodical(tag);

			await IncreaseChance(tag);

			return true;
		}

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

			// if we have a collectable && the collectable value is greater than or equal to 470: Priority 470
			if (tag.CollectableItem != null && tag.CollectableItem.Value >= 470)
			{
				return 470;
			}

			return -1;
		}
	}
}