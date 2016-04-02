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
	[GatheringRotation("SmartYield", 18, 500, 400, 300, 0)]
	public class SmartYieldGatheringRotation : SmartGatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members

		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			if (tag.CollectableItem != null)
			{
				return -1;
			}

			if (tag.GatherIncrease == GatherIncrease.Yield
			    || (tag.GatherIncrease == GatherIncrease.Auto && Core.Player.ClassLevel >= 40))
			{
				return 9001;
			}

			return -1;
		}

		#endregion

		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			var level = Core.Player.ClassLevel;

			if (GatheringManager.SwingsRemaining > 4 || ShouldForceUseRotation(tag, level))
			{
				if (Core.Player.CurrentGP >= 500 && level >= 40)
				{
					await tag.Cast(Ability.IncreaseGatherYield2);
					return await base.ExecuteRotation(tag);
				}

				if (Core.Player.CurrentGP >= 400 && level >= 30 && (level < 40 || Core.Player.MaxGP < 500))
				{
					await tag.Cast(Ability.IncreaseGatherYield);
					return await base.ExecuteRotation(tag);
				}

				if (Core.Player.CurrentGP >= 300 && level >= 25 && (level < 30 || Core.Player.MaxGP < 400))
				{
					await Wait();

					if (!tag.GatherItem.TryGatherItem())
					{
						return false;
					}

					await tag.Cast(Ability.AdditionalAttempt);
					return await base.ExecuteRotation(tag);
				}
			}

			return true;
		}
	}
}