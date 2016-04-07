namespace ExBuddy.Helpers
{
	using System.Collections.Generic;
	using ff14bot.Enums;

	internal static class Abilities
	{
		internal static readonly Dictionary<ClassJobType, Dictionary<Ability, uint>> Map =
			new Dictionary<ClassJobType, Dictionary<Ability, uint>>
			{
				{
					ClassJobType.Botanist,
					new Dictionary<Ability, uint>
					{
						{Ability.Stealth, 212},
						{Ability.Preparation, 213},
						{Ability.Toil, 214},
						{Ability.AdditionalAttempt, 215},
						{Ability.IncreaseGatherChanceQuality100, 216},
						{Ability.IncreaseGatherChance5, 218},
						{Ability.IncreaseGatherChance15, 220},
						{Ability.IncreaseGatherYield, 222},
						{Ability.IncreaseGatherYield2, 224},
						{Ability.IncreaseGatherQuality10, 225},
						{Ability.IncreaseGatherQuality30, 226},
						{Ability.Truth, 221},
						{Ability.IncreaseGatherChance50, 294},
						{Ability.IncreaseGatherChanceOnce15, 4086},
						{Ability.IncreaseGatherYieldOnce, 4087},
						{Ability.CollectorsGlove, 4088},
						{Ability.MethodicalAppraisal, 4089},
						{Ability.InstinctualAppraisal, 4090},
						{Ability.ImpulsiveAppraisal, 4091},
						{Ability.DiscerningEye, 4092},
						{Ability.UtmostCaution, 4093},
						{Ability.DeepBreath, 4094},
						{Ability.Luck, 4095},
						{Ability.IncreaseGatherQualityOnce10, 4096},
						{Ability.IncreaseGatherQualityOnce20, 4097},
						{Ability.SingleMind, 4098}
					}
				},
				{
					ClassJobType.Miner,
					new Dictionary<Ability, uint>
					{
						{Ability.Stealth, 229},
						{Ability.Preparation, 230},
						{Ability.Toil, 231},
						{Ability.AdditionalAttempt, 232},
						{Ability.IncreaseGatherChanceQuality100, 233},
						{Ability.IncreaseGatherChance5, 235},
						{Ability.IncreaseGatherChance15, 237},
						{Ability.IncreaseGatherYield, 239},
						{Ability.IncreaseGatherYield2, 241},
						{Ability.IncreaseGatherQuality10, 242},
						{Ability.IncreaseGatherQuality30, 243},
						{Ability.Truth, 238},
						{Ability.IncreaseGatherChance50, 295},
						{Ability.IncreaseGatherChanceOnce15, 4072},
						{Ability.IncreaseGatherYieldOnce, 4073},
						{Ability.CollectorsGlove, 4074},
						{Ability.MethodicalAppraisal, 4075},
						{Ability.InstinctualAppraisal, 4076},
						{Ability.ImpulsiveAppraisal, 4077},
						{Ability.DiscerningEye, 4078},
						{Ability.UtmostCaution, 4079},
						{Ability.DeepBreath, 4080},
						{Ability.Luck, 4081},
						{Ability.IncreaseGatherQualityOnce10, 4082},
						{Ability.IncreaseGatherQualityOnce20, 4083},
						{Ability.SingleMind, 4084}
					}
				}
			};
	}

	internal enum AbilityAura : short
	{
		None = -1,

		Stealth = 47,

		TruthOfForests = 221,

		TruthOfMountains = 222,

		DiscerningEye = 757,

		CollectorsGlove = 805
	}

	internal enum Ability : byte
	{
		None,

		Stealth, // = 229,212

		Preparation, // = 230,213

		Toil, // = 231,214

		AdditionalAttempt, // 232,215

		IncreaseGatherChanceQuality100, // 233,216

		IncreaseGatherChance5, // = 235,218

		IncreaseGatherChance15, // = 237,220

		IncreaseGatherYield, // = 239,222

		IncreaseGatherYield2, // = 241,224

		IncreaseGatherQuality10, // 242,225

		IncreaseGatherQuality30, // 243,226

		Truth, // = 238,221

		IncreaseGatherChance50, // = 295,294

		IncreaseGatherChanceOnce15, // 4072, 4086

		IncreaseGatherYieldOnce, // 4073, 4087

		CollectorsGlove, // = 4074,4088

		MethodicalAppraisal, // = 4075,4089

		InstinctualAppraisal, // = 4076,4090

		ImpulsiveAppraisal, // = 4077,4091

		DiscerningEye, // = 4078,4092

		UtmostCaution, // = 4079,4093

		DeepBreath, // = 4080,4094

		Luck, // = 4081,4095

		IncreaseGatherQualityOnce10, // = 4082, 4096

		IncreaseGatherQualityOnce20, // = 4083, 4097

		SingleMind // = 4084,4098
	}
}