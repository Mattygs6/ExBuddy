namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Threading.Tasks;

	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;

	using ff14bot.Managers;

	// Get Two ++
	[GatheringRotation("Collect470", 600, 31)]
	public sealed class Collect470GatheringRotation : CollectableGatheringRotation, IGetOverridePriority
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

			// if we have a collectable && the collectable value is greater than or equal to 470: Priority 470
			if (tag.CollectableItem != null && tag.CollectableItem.Value >= 470)
			{
				return 470;
			}

			return -1;
		}

		#endregion

		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			var gp = GameObjectManager.LocalPlayer.CurrentGP;
			if (gp >= 600)
			{
				await Discerning(tag);

				await AppraiseAndRebuff(tag);
				await AppraiseAndRebuff(tag);

				await Methodical(tag);
			}
			else
			{
				tag.Logger.Warn("Using alternate rotation to collect one or two due to current GP: {0} being less than required GP: {1}", gp, 600);
				// Less than 600 GP collect 1-2 rotation
				await UtmostImpulsive(tag);

				if (HasDiscerningEye)
				{
					await UtmostSingleMindMethodical(tag);
				}
				else
				{
					await UtmostCaution(tag);
					await AppraiseAndRebuff(tag);
				}

				await Methodical(tag);
				await Methodical(tag);
			}

			await IncreaseChance(tag);
			return true;
		}
	}
}