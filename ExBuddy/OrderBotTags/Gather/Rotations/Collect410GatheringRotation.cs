namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Threading.Tasks;
	using Attributes;
	using Interfaces;
	using ff14bot;

	[GatheringRotation("Collect410", 30, 600)]
	public sealed class Collect410GatheringRotation : CollectableGatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members
		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			// if we have a collectable && the collectable value is greater than or equal to 410: Priority 410
			if (tag.CollectableItem != null && tag.CollectableItem.Value >= 410)
			{
				return 410;
			}
			return -1;
		}
		#endregion
		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			if (tag.IsUnspoiled())
			{
				await AppraiseAndRebuff(tag);
				await AppraiseAndRebuff(tag);
				await Methodical(tag);
			}
			else
			{
				if (Core.Player.CurrentGP >= 600)
				{
					await AppraiseAndRebuff(tag);
					await AppraiseAndRebuff(tag);
					await Methodical(tag);
					return true;
				}

				await Impulsive(tag);
				await Impulsive(tag);
				await Methodical(tag);
			}
			return true;
		}
	}
}