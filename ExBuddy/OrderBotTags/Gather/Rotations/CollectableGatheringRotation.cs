namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System;
	using System.Threading.Tasks;

	using Buddy.Coroutines;

	using ExBuddy.Helpers;
	using ExBuddy.Windows;

	using ff14bot;
	using ff14bot.Helpers;
	using ff14bot.Managers;
	using ff14bot.RemoteWindows;

	public abstract class CollectableGatheringRotation : GatheringRotation
	{
		protected static GatheringMasterpiece MasterpieceWindow = new GatheringMasterpiece();

		public override bool ShouldForceGather
		{
			get
			{
				return Poi.Current != null && Poi.Current.Type == PoiType.Gather
						&& Poi.Current.Name.IndexOf("ephemeral", StringComparison.InvariantCultureIgnoreCase) == -1
						&& Poi.Current.Name.IndexOf("unspoiled", StringComparison.InvariantCultureIgnoreCase) == -1;
			}
		}

		protected int CurrentRarity
		{
			get
			{
				return MasterpieceWindow.CurrentRarity;
			}
		}

		protected bool HasDiscerningEye
		{
			get
			{
				return Core.Player.HasAura((int)AbilityAura.DiscerningEye);
			}
		}

		public override async Task<bool> Prepare(ExGatherTag tag)
		{
			await tag.CastAura(Ability.CollectorsGlove, AbilityAura.CollectorsGlove);

			var ticks = 0;
			do
			{
				await Wait();

				if (!tag.GatherItem.TryGatherItem())
				{
					return false;
				}
			}
			while (ticks++ < 10 && !await MasterpieceWindow.Refresh(3000) && Behaviors.ShouldContinue);

			if (ticks > 10)
			{
				tag.Logger.Error("Timed out during collectable preparation");
			}

			return true;
		}

		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			await DiscerningMethodical(tag);
			await DiscerningMethodical(tag);
			await DiscerningMethodical(tag);

			await IncreaseChance(tag);
			return true;
		}

		public override async Task<bool> Gather(ExGatherTag tag)
		{
			tag.StatusText = "Gathering collectable items";

			var rarity = CurrentRarity;
			while (tag.Node.CanGather && GatheringManager.SwingsRemaining > 0 && rarity > 0 && Behaviors.ShouldContinue)
			{
				while (!SelectYesNoItem.IsOpen && tag.Node.CanGather && GatheringManager.SwingsRemaining > 0 && rarity > 0
						&& Behaviors.ShouldContinue)
				{
					if (!MasterpieceWindow.IsValid)
					{
						await MasterpieceWindow.Refresh(3000);
					}

					var itemRarity = rarity = CurrentRarity;
					if (SelectYesNoItem.CollectabilityValue >= itemRarity)
					{
						await Coroutine.Wait(1000, () => SelectYesNoItem.CollectabilityValue < itemRarity);
					}

					if (MasterpieceWindow.IsValid)
					{
						MasterpieceWindow.Collect();
					}

					await Coroutine.Wait(1000, () => SelectYesNoItem.IsOpen);
				}

				await Coroutine.Yield();
				var swingsRemaining = GatheringManager.SwingsRemaining - 1;

				while (SelectYesNoItem.IsOpen && rarity > 0 && Behaviors.ShouldContinue)
				{
					var itemRarity = rarity = CurrentRarity;
					if (SelectYesNoItem.CollectabilityValue < itemRarity)
					{
						await Coroutine.Wait(4000, () => SelectYesNoItem.CollectabilityValue >= itemRarity);
					}

					tag.Logger.Info(
						"Collected item: {0}, value: {1} at {2} ET",
						tag.GatherItem.ItemData.EnglishName,
						SelectYesNoItem.CollectabilityValue,
						WorldManager.EorzaTime);

					SelectYesNoItem.Yes();
					await Coroutine.Wait(1000, () => !SelectYesNoItem.IsOpen);
				}

				var ticks = 0;
				while (swingsRemaining != GatheringManager.SwingsRemaining && ticks++ < 60 && Behaviors.ShouldContinue)
				{
					await Coroutine.Yield();
				}
			}

			tag.StatusText = "Gathering collectable items complete";

			return true;
		}

		protected override async Task<bool> IncreaseChance(ExGatherTag tag)
		{
			var level = Core.Player.ClassLevel;
			if (Core.Player.CurrentGP >= 100 && tag.GatherItem.Chance < 95)
			{
				if (level >= 23 && GatheringManager.SwingsRemaining == 1)
				{
					await tag.Cast(Ability.IncreaseGatherChanceOnce15);
					return true;
				}

				await tag.Cast(Ability.IncreaseGatherChance15);
				return true;
			}

			if (Core.Player.CurrentGP >= 50 && tag.GatherItem.Chance < 100)
			{
				if (level >= 23 && GatheringManager.SwingsRemaining == 1)
				{
					await tag.Cast(Ability.IncreaseGatherChanceOnce15);
					return true;
				}

				await tag.Cast(Ability.IncreaseGatherChance5);
				return true;
			}

			return true;
		}

		protected async Task AppraiseAndRebuff(ExGatherTag tag)
		{
			await Impulsive(tag);

			if (HasDiscerningEye)
			{
				await tag.Cast(Ability.SingleMind);
			}
			else
			{
				await tag.Cast(Ability.DiscerningEye);
			}
		}

		protected async Task Discerning(ExGatherTag tag)
		{
			await tag.Cast(Ability.DiscerningEye);
		}

		protected async Task DiscerningMethodical(ExGatherTag tag)
		{
			await tag.Cast(Ability.DiscerningEye);
			await tag.Cast(Ability.MethodicalAppraisal);
		}

		protected async Task DiscerningImpulsive(ExGatherTag tag)
		{
			await tag.Cast(Ability.DiscerningEye);
			await tag.Cast(Ability.ImpulsiveAppraisal);
		}

		protected async Task DiscerningUtmostMethodical(ExGatherTag tag)
		{
			await tag.Cast(Ability.DiscerningEye);
			await tag.Cast(Ability.UtmostCaution);
			await tag.Cast(Ability.MethodicalAppraisal);
		}

		protected async Task UtmostCaution(ExGatherTag tag)
		{
			await tag.Cast(Ability.UtmostCaution);
		}

		protected async Task UtmostImpulsive(ExGatherTag tag)
		{
			await tag.Cast(Ability.UtmostCaution);
			await tag.Cast(Ability.ImpulsiveAppraisal);
		}

		protected async Task UtmostMethodical(ExGatherTag tag)
		{
			await tag.Cast(Ability.UtmostCaution);
			await tag.Cast(Ability.MethodicalAppraisal);
		}

		protected async Task Impulsive(ExGatherTag tag)
		{
			await tag.Cast(Ability.ImpulsiveAppraisal);
		}

		protected async Task Methodical(ExGatherTag tag)
		{
			await tag.Cast(Ability.MethodicalAppraisal);
		}

		protected async Task SingleMindImpulsive(ExGatherTag tag)
		{
			await tag.Cast(Ability.SingleMind);
			await tag.Cast(Ability.ImpulsiveAppraisal);
		}

		protected async Task SingleMindMethodical(ExGatherTag tag)
		{
			await tag.Cast(Ability.SingleMind);
			await tag.Cast(Ability.MethodicalAppraisal);
		}

		protected async Task SingleMindUtmostMethodical(ExGatherTag tag)
		{
			await tag.Cast(Ability.SingleMind);
			await tag.Cast(Ability.UtmostCaution);
			await tag.Cast(Ability.MethodicalAppraisal);
		}
	}
}