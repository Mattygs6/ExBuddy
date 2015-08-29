namespace ExBuddy.OrderBotTags
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
                                {
                                    Ability
                                    .Stealth,
                                    212
                                },
                                {
                                    Ability
                                    .IncreaseGatherChance5,
                                    218
                                },
                                {
                                    Ability.IncreaseGatherYield,
                                    222
                                },
                                {
                                    Ability.IncreaseGatherYield2,
                                    224
                                },
                                {
                                    Ability
                                    .Truth,
                                    221
                                },
                                {
                                    Ability
                                    .CollectorsGlove,
                                    4088
                                },
                                {
                                    Ability
                                    .MethodicalAppraisal,
                                    4089
                                },
                                {
                                    Ability
                                    .ImpulsiveAppraisal,
                                    4091
                                },
                                {
                                    Ability
                                    .DiscerningEye,
                                    4092
                                },
                                {
                                    Ability
                                    .UtmostCaution,
                                    4093
                                },
                                {
                                    Ability
                                    .DeepBreath,
                                    4094
                                },
                                {
                                    Ability
                                    .SingleMind,
                                    4098
                                }
                            }
                    },
                    {
                        ClassJobType.Miner,
                        new Dictionary<Ability, uint>
                            {
                                {
                                    Ability
                                    .Stealth,
                                    229
                                },
                                {
                                    Ability
                                    .IncreaseGatherChance5,
                                    235
                                },
                                {
                                    Ability.IncreaseGatherYield,
                                    239
                                },
                                {
                                    Ability.IncreaseGatherYield2,
                                    241
                                },
                                {
                                    Ability
                                    .Truth,
                                    238
                                },
                                {
                                    Ability
                                    .CollectorsGlove,
                                    4074
                                },
                                {
                                    Ability
                                    .MethodicalAppraisal,
                                    4075
                                },
                                {
                                    Ability
                                    .ImpulsiveAppraisal,
                                    4077
                                },
                                {
                                    Ability
                                    .DiscerningEye,
                                    4078
                                },
                                {
                                    Ability
                                    .UtmostCaution,
                                    4079
                                },
                                {
                                    Ability
                                    .DeepBreath,
                                    4080
                                },
                                {
                                    Ability
                                    .SingleMind,
                                    4084
                                }
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

        IncreaseGatherChance5, // = 235,218

        IncreaseGatherYield, // = 239,222

        IncreaseGatherYield2, // = 241,224

        Truth, // = 238,221

        CollectorsGlove, // = 4074,4088

        MethodicalAppraisal, // = 4075,4089

        ImpulsiveAppraisal, // = 4077,4091

        DiscerningEye, // = 4078,4092

        UtmostCaution, // = 4079,4093

        DeepBreath, // = 4080,4094

        SingleMind, // = 4084,4098
    }
}
