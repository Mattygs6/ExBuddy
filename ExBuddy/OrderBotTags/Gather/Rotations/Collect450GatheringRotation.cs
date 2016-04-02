namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Threading.Tasks;
	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;
	using ff14bot.Managers;

	// Get Three
	[GatheringRotation("Collect450", 30, 600, 400, 200)]
	public sealed class Collect450GatheringRotation : CollectableGatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members

		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			// if we have a collectable && the collectable value is greater than or equal to 450: Priority 450
			if (tag.CollectableItem != null && tag.CollectableItem.Value >= 450)
			{
				return 450;
			}

			return -1;
		}

		#endregion

		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			var gp = GameObjectManager.LocalPlayer.CurrentGP;
			if (gp >= 400)
			{
				if (gp < 600)
				{
					tag.Logger.Warn(
						"Using alternate rotation to collect two due to CurrentGP: {0} being less than RequiredGP: {1}",
						gp,
						600);

					await DiscerningMethodical(tag);
					await DiscerningMethodical(tag);
					await Methodical(tag);
				}
				else
				{
					await DiscerningMethodical(tag);
					await DiscerningMethodical(tag);
					await SingleMindMethodical(tag);
				}

				await IncreaseChance(tag);
			}
			else
			{
				tag.Logger.Warn(
					"Using alternate rotation to collect one due to CurrentGP: {0} being less than RequiredGP: {1}",
					gp,
					400);
				// Less than 400 GP collect 1 rotation
				await UtmostMethodical(tag);
				await UtmostMethodical(tag);
				await Methodical(tag);
				await Methodical(tag);

				await IncreaseChance(tag);
			}

			return true;
		}
	}
}