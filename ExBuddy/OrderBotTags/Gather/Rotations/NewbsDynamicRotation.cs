namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System;
	using System.Threading.Tasks;
	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;
	using ff14bot;

	[GatheringRotation("NewbCollect", 30, 600)]
	public sealed class NewbCollectGatheringRotation : CollectableGatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members

		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			// if we have a collectable Priority 0
			if (tag.CollectableItem != null && tag.CollectableItem.Value == 0)
			{
				return 80;
			}

			return -1;
		}

		#endregion

		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			var level = Core.Player.ClassLevel;

			if (level >= 57)
			{
				if (tag.CollectableItem.PlusPlus == 2)
				{
					await CallRotation(tag, "Get Two++", GetTwoPlusPlus);
					return true;
				}

				if (tag.CollectableItem.PlusPlus == 1)
				{
					await CallRotation(tag, "Get One++", GetOnePlusPlus);
					return true;
				}
			}

			await DiscerningMethodical(tag);
			var rarity = CurrentRarity;
			tag.Logger.Info("Post non-plus Rarity: " + rarity);

			if (rarity >= 119 && rarity <= 124)
			{
				if (level >= 53)
				{
					await CallRotation(tag, "Try Harder", TryHarder);
				}
				else
				{
					await CallRotation(tag, "Get Two", GetTwo);
				}

				return true;
			}

			if (rarity >= 125 && rarity <= 134)
			{
				if (level >= 53)
				{
					await CallRotation(tag, "Try Hard", TryHard);
				}
				else
				{
					await CallRotation(tag, "Get Two", GetTwo);
				}

				return true;
			}

			if (rarity >= 135 && rarity <= 137)
			{
				if (level >= 51)
				{
					await CallRotation(tag, "Get One", GetOne);
				}
				else
				{
					await CallRotation(tag, "Get Two", GetTwo);
				}

				return true;
			}

			if (rarity >= 138 && rarity <= 140)
			{
				if (level >= 57)
				{
					await CallRotation(tag, "Get One+", GetOnePlus);
				}
				else if (level >= 51)
				{
					await CallRotation(tag, "Get One", GetOne);
				}
				else
				{
					await CallRotation(tag, "Get Two", GetTwo);
				}

				return true;
			}

			if (rarity >= 141 && rarity <= 149)
			{
				if (level >= 57)
				{
					tag.Logger.Info(
						"Hey! Listen! You can update this item to use Get One++!!! Using Rotation: Get One+ for now... :'(");
					await CallRotation(tag, "Get One+", GetOnePlus);
				}
				else if (level >= 51)
				{
					await CallRotation(tag, "Get One", GetOne);
				}
				else
				{
					await CallRotation(tag, "Get Two", GetTwo);
				}

				return true;
			}

			if (rarity >= 150 && rarity <= 155)
			{
				await CallRotation(tag, "Get Two", GetTwo);
				return true;
			}

			if (rarity >= 156 && rarity <= 160)
			{
				if (level >= 57)
				{
					await CallRotation(tag, "Get Two+", GetTwoPlus);
				}
				else
				{
					await CallRotation(tag, "Get Two", GetTwo);
				}

				return true;
			}

			if (rarity >= 161 && rarity <= 168)
			{
				if (level >= 57)
				{
					tag.Logger.Info(
						"Hey! Listen! You can update this item to use Get Two++!!! Using Rotation: Get Two+ for now... :'(");
					await CallRotation(tag, "Get Two+", GetTwoPlus);
				}
				else
				{
					await CallRotation(tag, "Get Two", GetTwo);
				}

				return true;
			}

			if (rarity >= 169)
			{
				if (level >= 57)
				{
					await CallRotation(tag, "Get Three", GetThree);
				}
				else
				{
					await CallRotation(tag, "Get Two", GetTwo);
				}

				return true;
			}

			return false;
		}

		public async Task<bool> GetOne(ExGatherTag tag)
		{
			//Get One - Level 51 Minimum           
			await DiscerningMethodical(tag);
			await UtmostMethodical(tag);
			await UtmostMethodical(tag);
			await IncreaseChance(tag);
			return true;
		}

		public async Task<bool> GetOnePlus(ExGatherTag tag)
		{
			//Get One+ - Level 57 Minimum
			await UtmostCaution(tag);
			await AppraiseAndRebuff(tag);
			await Methodical(tag);
			await UtmostMethodical(tag);
			await IncreaseChance(tag);
			return true;
		}

		public async Task<bool> GetOnePlusPlus(ExGatherTag tag)
		{
			//Get One++ - Level 57 Minimum
			await UtmostCaution(tag);
			await AppraiseAndRebuff(tag);
			await Methodical(tag);
			await UtmostCaution(tag);
			await AppraiseAndRebuff(tag);
			await Methodical(tag);
			await IncreaseChance(tag);
			return true;
		}

		public async Task<bool> GetThree(ExGatherTag tag)
		{
			//Get Three - Level 57 Minimum
			await DiscerningMethodical(tag);
			await SingleMindMethodical(tag);
			await IncreaseChance(tag);
			return true;
		}

		public async Task<bool> GetTwo(ExGatherTag tag)
		{
			//Get Two - Level 50 Minimum
			await DiscerningMethodical(tag);
			await DiscerningMethodical(tag);
			await IncreaseChance(tag);
			return true;
		}

		public async Task<bool> GetTwoPlus(ExGatherTag tag)
		{
			//Get Two+ - Level 57 Minimum
			await Discerning(tag);
			await AppraiseAndRebuff(tag);
			await Methodical(tag);
			await IncreaseChance(tag);
			return true;
		}

		public async Task<bool> GetTwoPlusPlus(ExGatherTag tag)
		{
			//Get Two++ - Level 57 Minimum
			await Discerning(tag);
			await AppraiseAndRebuff(tag);
			await AppraiseAndRebuff(tag);
			await Methodical(tag);
			await IncreaseChance(tag);
			return true;
		}

		public async Task<bool> TryHard(ExGatherTag tag)
		{
			//Try Hard - Level 53 Minimum           
			await UtmostImpulsive(tag);

			if (HasDiscerningEye)
			{
				tag.Logger.Info("Discerning Eye Proc!");
				await UtmostMethodical(tag);
				await DiscerningMethodical(tag);
				await IncreaseChance(tag);
			}
			else
			{
				tag.Logger.Info("No Discerning Eye Proc!");
				await DiscerningImpulsive(tag);
				await UtmostMethodical(tag);
				await IncreaseChance(tag);
			}

			return true;
		}

		public async Task<bool> TryHarder(ExGatherTag tag)
		{
			//Try Harder - Level 53 Minimum            
			await DiscerningImpulsive(tag);
			await UtmostImpulsive(tag);
			await UtmostMethodical(tag);
			await IncreaseChance(tag);
			return true;
		}

		private static async Task CallRotation(ExGatherTag tag, string rotationName, Func<ExGatherTag, Task<bool>> callBack)
		{
			tag.Logger.Info("Using Rotation: " + rotationName);
			await callBack(tag);
			tag.Logger.Info("Exiting Rotation: " + rotationName);
		}
	}
}