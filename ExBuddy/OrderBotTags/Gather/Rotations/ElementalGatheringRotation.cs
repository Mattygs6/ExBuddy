namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Linq;
	using System.Threading.Tasks;

	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;

	using ff14bot;
	using ff14bot.Managers;

	//Name, RequiredGp, RequiredTime
	[GatheringRotation("Elemental", 0, 0)]
	public class ElementalGatheringRotation : SmartGatheringRotation, IGetOverridePriority
	{
		public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
		{
			if (Core.Player.CurrentGP < 400 || tag.GatherItemIsFallback)
			{
				return true;
			}

			await Wait();

			var ward = WardSkills.FirstOrDefault(w => Actionmanager.CanCast(w, Core.Player));

			if (ward > 0)
			{
				Actionmanager.DoAction(ward, Core.Player);
				await IncreaseChance(tag);
			}

			return true;
		}

		int IGetOverridePriority.GetOverridePriority(GatherCollectableTag tag)
		{
			// Don't use unless ward increases item yield.
			if (!DoesWardIncreaseItemYield(tag))
			{
				return -1;
			}

			if (!tag.GatherItem.IsUnknown && tag.GatherItem.ItemId < 20)
			{
				return 10000;
			}

			return -1;
		}

		protected bool DoesWardIncreaseItemYield(GatherCollectableTag tag)
		{
			if (tag.GatherItem.ItemId < 8)
			{
				return true;
			}

			if (tag.GatherItem.ItemId < 14 && Core.Player.ClassLevel >= 41)
			{
				return true;
			}

			if (Core.Player.ClassLevel >= 50)
			{
				return true;
			}

			return false;
		}
	}
}