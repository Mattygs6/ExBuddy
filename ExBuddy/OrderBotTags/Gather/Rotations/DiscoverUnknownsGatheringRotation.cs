namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Linq;
	using System.Threading.Tasks;
	using ExBuddy.Attributes;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ff14bot;
	using ff14bot.Managers;

	// TODO: if can peek, then we need to allow it to redo beforegather logic
	//Name, RequiredTime, RequiredGpBreakpoints
	[GatheringRotation("DiscoverUnknowns", 12, 250)]
	public class DiscoverUnknownsGatheringRotation : GatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members

		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			if (tag.GatherItem == null)
			{
				return -1;
			}

			if (tag.GatherItem.IsUnknown || (tag.IsUnspoiled() && tag.GatherItem.Chance == 25))
			{
				return int.MaxValue;
			}

			return -1;
		}

		#endregion

		public override async Task<bool> Prepare(ExGatherTag tag)
		{
			var unknownItems = GatheringManager.GatheringWindowItems.Where(i => i.IsUnknownChance() && i.Amount > 0).ToArray();

			if (tag.IsUnspoiled() && Core.Player.CurrentGP >= 550 && unknownItems.Length > 1)
			{
				await tag.Cast(Ability.Toil);
			}

			return await base.Prepare(tag);
		}
	}
}