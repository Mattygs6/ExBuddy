namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using ExBuddy.Attributes;
	using ff14bot;
	using ff14bot.Managers;
	using ff14bot.Objects;

	// Purposely not putting attribute or interface for overriding, This is for backwards compatibility only override turned off automatically.
	public sealed class GatheringSkillOrderGatheringRotation : SmartGatheringRotation
	{
		// ReSharper disable once InconsistentNaming
		private static readonly GatheringRotationAttribute attributes = new GatheringRotationAttribute("GatheringSkillOrder");

		public override GatheringRotationAttribute Attributes
		{
			get { return attributes; }
		}

		public override bool CanBeOverriden
		{
			get { return false; }
		}

		public override async Task<bool> ExecuteRotation(ExGatherTag tag)
		{
			var gpRequired = 0U;
			var skillList = new List<SpellData>();
			foreach (var gatheringSkill in tag.GatheringSkillOrder.GatheringSkills)
			{
				// Ignoring times to cast.... no skills would ever be cast more than once.
				SpellData spellData;

				if (!Actionmanager.CurrentActions.TryGetValue(gatheringSkill.SpellName, out spellData))
				{
					Actionmanager.CurrentActions.TryGetValue(gatheringSkill.SpellId, out spellData);
				}

				if (spellData == null)
				{
					tag.Logger.Warn("Unable to find skill -> Name: {0}, Id: {1}", gatheringSkill.SpellName, gatheringSkill.SpellId);
				}
				else
				{
					skillList.Add(spellData);
					gpRequired += spellData.Cost;
				}
			}
			if (!tag.GatheringSkillOrder.AllOrNone || gpRequired <= Core.Player.CurrentGP)
			{
				foreach (var skill in skillList)
				{
					if (Core.Player.CurrentGP > skill.Cost)
					{
						await tag.Cast(skill.Id);
					}
				}
			}

			return true;
		}
	}
}