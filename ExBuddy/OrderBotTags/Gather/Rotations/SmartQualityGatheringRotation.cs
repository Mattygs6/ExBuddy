namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Threading.Tasks;
	using ExBuddy.Attributes;
	using ExBuddy.Enumerations;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ff14bot;
	using ff14bot.Managers;

	//Name, RequiredTime, RequiredGpBreakpoints
	[GatheringRotation("SmartQuality", 18, 300, 100, 0)]
	public class SmartQualityGatheringRotation : SmartGatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members

		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			if (tag.CollectableItem != null)
			{
				return -1;
			}

			if (tag.GatherItem != null && tag.GatherItem.HqChance < 1)
			{
				return -1;
			}

			if (tag.GatherIncrease == GatherIncrease.Quality
			    || (tag.GatherIncrease == GatherIncrease.Auto && Core.Player.ClassLevel >= 15 && Core.Player.ClassLevel < 40))
			{
				return 9001;
			}

			return -1;
		}

		#endregion

		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			if (Core.Player.CurrentGP >= 300 && GatheringManager.SwingsRemaining > 4)
			{
				await tag.Cast(Ability.IncreaseGatherQuality30);
				await base.ExecuteRotation(tag);

				if (tag.GatherItem.Chance == 100 && Core.Player.CurrentGP >= 300 && GatheringManager.SwingsRemaining == 5)
				{
					await Wait();

					if (!tag.GatherItem.TryGatherItem())
					{
						return false;
					}

					await tag.Cast(Ability.AdditionalAttempt);
				}

				return true;
			}

			// Approx 30 gp or more between running to nodes, we are basically capped here so just use 100 gp
			if (Core.Player.CurrentGP >= Core.Player.MaxGP - 30)
			{
				await tag.Cast(Ability.IncreaseGatherQuality10);
				return true;
			}

			return true;
		}
	}
}