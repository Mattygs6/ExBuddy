namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Threading.Tasks;
	using Attributes;
	using Interfaces;
	using ff14bot;

	[GatheringRotation("Collect460", 33, 600)]
	public sealed class Collect460GatheringRotation : CollectableGatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members
		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			// if we have a collectable && the collectable value is greater than or equal to 460: Priority 460
			if (tag.CollectableItem != null && tag.CollectableItem.Value >= 460)
			{
				return 460;
			}
			return -1;
		}
		#endregion
		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			if (tag.IsUnspoiled())
			{
				await SingleMindMethodical(tag);
				await SingleMindMethodical(tag);
				await UtmostCaution(tag);
				await Methodical(tag);
				await UtmostCaution(tag);
				await Methodical(tag);
			}
			else
			{
				if (Core.Player.CurrentGP >= 600)
				{
					await SingleMindMethodical(tag);
					await SingleMindMethodical(tag);
					await UtmostCaution(tag);
					await Methodical(tag);
					await UtmostCaution(tag);
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