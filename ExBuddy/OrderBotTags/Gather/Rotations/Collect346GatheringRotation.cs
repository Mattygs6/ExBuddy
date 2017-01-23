namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Threading.Tasks;
	using Attributes;
	using Interfaces;
	using ff14bot;

	[GatheringRotation("Collect346", 30, 600)]
	public sealed class Collect346GatheringRotation : CollectableGatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members
		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			// if we have a collectable && the collectable value is greater than or equal to 346: Priority 346
			if (tag.CollectableItem != null && tag.CollectableItem.Value >= 346)
			{
				return 346;
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
				await SingleMindMethodical(tag);
			}
			else
			{
				if (Core.Player.CurrentGP >= 600)
				{
					await SingleMindMethodical(tag);
					await SingleMindMethodical(tag);
					await SingleMindMethodical(tag);
					return true;
				}
				
				await Methodical(tag);
				await Methodical(tag);
				await Methodical(tag);
			}
			return true;
		}
	}
}