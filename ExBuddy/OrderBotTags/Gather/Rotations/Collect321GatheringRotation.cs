namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Threading.Tasks;
	using Attributes;
	using Interfaces;
	using ff14bot;

	[GatheringRotation("Collect321", 30, 600)]
	public sealed class Collect321GatheringRotation : CollectableGatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members
		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			// if we have a collectable && the collectable value is greater than or equal to 321: Priority 321
			if (tag.CollectableItem != null && tag.CollectableItem.Value >= 321)
			{
				return 321;
			}
			return -1;
		}
		#endregion
		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			if (tag.IsUnspoiled())
			{
				await SingleMindImpulsive(tag);
				await SingleMindImpulsive(tag);
				await SingleMindMethodical(tag);
			}
			else
			{
				if (Core.Player.CurrentGP >= 600)
				{
					await SingleMindImpulsive(tag);
					await SingleMindImpulsive(tag);
					await SingleMindMethodical(tag);
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