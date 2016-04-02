namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Threading.Tasks;
	using Buddy.Coroutines;
	using ExBuddy.Attributes;
	using ExBuddy.Helpers;
	using ff14bot;
	using ff14bot.Managers;

	[GatheringRotation("GetTwoHQ", 18, 600)]
	public sealed class GetTwoHqGatheringRotation : GatheringRotation
	{
		public override bool CanBeOverriden
		{
			get { return false; }
		}

		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			await tag.Cast(Ability.Toil);

			return await base.ExecuteRotation(tag);
		}

		public override async Task<bool> Gather(ExGatherTag tag)
		{
			tag.StatusText = "Gathering items";

			while (tag.Node.CanGather && GatheringManager.SwingsRemaining > tag.SwingsRemaining && Behaviors.ShouldContinue)
			{
				await Wait();

				if (GatheringManager.GatheringCombo == 4 && GatheringManager.SwingsRemaining > tag.SwingsRemaining)
				{
					await tag.Cast(Ability.IncreaseGatherChanceQuality100);
					await tag.Cast(Ability.IncreaseGatherYieldOnce);
					await Wait();
				}

				// No way to tell if we missed the first swing afaik... so we have to check if we have a combo of 0 anytime after the second swing.
				if (GatheringManager.GatheringCombo == 0 && GatheringManager.SwingsRemaining < 5 && !Core.Player.HasAura(220))
				{
					await tag.Cast(Ability.IncreaseGatherQuality10);
				}

				if (!await tag.ResolveGatherItem())
				{
					return false;
				}

				var swingsRemaining = GatheringManager.SwingsRemaining - 1;

				if (!tag.GatherItem.TryGatherItem())
				{
					return false;
				}

				var ticks = 0;
				while (swingsRemaining != GatheringManager.SwingsRemaining && ticks++ < 60 && Behaviors.ShouldContinue)
				{
					await Coroutine.Yield();
				}
			}

			tag.StatusText = "Gathering items complete";

			return true;
		}

		protected override async Task<bool> IncreaseChance(ExGatherTag tag)
		{
			var level = Core.Player.ClassLevel;
			if (Core.Player.CurrentGP >= 250 && tag.GatherItem.Chance < 51 && level > 10)
			{
				return await tag.Cast(Ability.IncreaseGatherChance50);
			}

			if (Core.Player.CurrentGP >= 100 && tag.GatherItem.Chance < 95 && level > 4)
			{
				if (level >= 23 && GatheringManager.SwingsRemaining == 1)
				{
					return await tag.Cast(Ability.IncreaseGatherChanceOnce15);
				}

				return await tag.Cast(Ability.IncreaseGatherChance15);
			}

			if (Core.Player.CurrentGP >= 50 && tag.GatherItem.Chance < 100 && level > 3)
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