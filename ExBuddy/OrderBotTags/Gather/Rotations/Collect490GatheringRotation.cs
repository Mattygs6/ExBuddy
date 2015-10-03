namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Threading.Tasks;

	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;

	using ff14bot.Managers;

	[GatheringRotation("Collect490", 600, 31)]
	public sealed class Collect490GatheringRotation : CollectableGatheringRotation, IGetOverridePriority
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

			// if we have a collectable && the collectable value is greater than or equal to 490: Priority 490
			if (tag.CollectableItem != null && tag.CollectableItem.Value >= 490)
			{
				return 490;
			}

			return -1;
		}

		#endregion

		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
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
			await SingleMindMethodical(tag); // TODO: display message we are using alternate since not enough gp

			await IncreaseChance(tag);

			return true;
		}
	}
}