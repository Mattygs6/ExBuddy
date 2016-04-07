namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Threading.Tasks;
	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;

	[GatheringRotation("Collect505", 31, 600, 400)]
	public sealed class Collect505GatheringRotation : CollectableGatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members

		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			// if we have a collectable && the collectable value is greater than or equal to 505: Priority 505
			if (tag.CollectableItem != null && tag.CollectableItem.Value >= 505)
			{
				return 505;
			}

			return -1;
		}

		#endregion

		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			await UtmostCaution(tag);
			await AppraiseAndRebuff(tag);
			await UtmostMethodical(tag);
			await Methodical(tag);
			await SingleMindMethodical(tag); // TODO: display message we are using alternate since not enough gp

			await IncreaseChance(tag);

			return true;
		}
	}
}