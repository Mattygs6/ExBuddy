namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Threading.Tasks;
	using ExBuddy.Attributes;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ff14bot;
	using ff14bot.Managers;

	//Name, RequiredTime, RequiredGpBreakpoints
	[GatheringRotation("Unspoiled", 21, 500, 0)]
	public sealed class UnspoiledGatheringRotation : GatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members

		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			if (tag.IsUnspoiled() && tag.CollectableItem == null)
			{
				return 8000;
			}

			return -1;
		}

		#endregion

		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			if (Core.Player.CurrentGP >= 500)
			{
				await tag.Cast(Ability.IncreaseGatherYield2);
			}

			return await base.ExecuteRotation(tag);
		}

		public override async Task<bool> Prepare(ExGatherTag tag)
		{
			await Wait();

			return tag.GatherItem.TryGatherItem() && await base.Prepare(tag);
		}

		protected override async Task<bool> IncreaseChance(ExGatherTag tag)
		{
			var level = Core.Player.ClassLevel;
			if (Core.Player.CurrentGP >= 100 && tag.GatherItem.Chance < 95)
			{
				if (level >= 23 && GatheringManager.SwingsRemaining == 1)
				{
					return await tag.Cast(Ability.IncreaseGatherChanceOnce15);
				}

				return await tag.Cast(Ability.IncreaseGatherChance15);
			}

			if (Core.Player.CurrentGP >= 50 && tag.GatherItem.Chance < 100)
			{
				if (level >= 23 && GatheringManager.SwingsRemaining == 1)
				{
					return await tag.Cast(Ability.IncreaseGatherChanceOnce15);
				}

				return await tag.Cast(Ability.IncreaseGatherChance5);
			}

			return true;
		}
	}
}