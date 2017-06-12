namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Threading.Tasks;
	using Attributes;
	using Interfaces;
	using ff14bot;

	[GatheringRotation("Collect459", 30, 600)]
	public sealed class Collect459GatheringRotation : CollectableGatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members
		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			// if we have a collectable && the collectable value is greater than or equal to 459: Priority 459
			if (tag.CollectableItem != null && tag.CollectableItem.Value >= 459)
			{
				return 459;
			}
			return -1;
		}
		#endregion
		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			if (tag.IsUnspoiled())
			{
				await SingleMindMethodical(tag);
				await DiscerningMethodical(tag);
				await DiscerningMethodical(tag);
			}
			else
			{
				if (Core.Player.CurrentGP >= 600)
				{
					await SingleMindMethodical(tag);
					await DiscerningMethodical(tag);
					await DiscerningMethodical(tag);
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