namespace ExBuddy.OrderBotTags.Behaviors.Objects
{
    using System.Collections.Generic;
    using System.Linq;
    using Clio.Utilities;
    using ExBuddy.GameObjects.Npcs;
    using ExBuddy.Interfaces;

    internal static class Data
    {
        public static readonly Dictionary<Locations, IList<INpc>> NpcMap = new Dictionary<Locations, IList<INpc>>
        {
            {
                Locations.MorDhona,
                new INpc[]
                {
                    new MasterPieceSupply
                    {
                        AetheryteId = 24,
                        ZoneId = 156,
                        Location = new Vector3("50.33948, 31.13618, -737.4532"),
                        NpcId = 1013396
                    },
                    new ShopExchangeCurrency
                    {
                        AetheryteId = 24,
                        ZoneId = 156,
                        Location = new Vector3("47.34875, 31.15659, -737.4838"),
                        NpcId = 1013397
                    }
                }
            },
            {
                Locations.Idyllshire,
                new INpc[]
                {
                    new MasterPieceSupply
                    {
                        AetheryteId = 75,
                        ZoneId = 478,
                        Location = new Vector3("-18.29561, 206.5211, 45.12088"),
                        NpcId = 1012300
                    },
                    new ShopExchangeCurrency
                    {
                        AetheryteId = 75,
                        ZoneId = 478,
                        Location = new Vector3("-20.6455, 206.5211, 47.25714"),
                        NpcId = 1012301
                    }
                }
            },
            {
                Locations.LimsaLominsaLowerDecks,
                new INpc[]
                {
                    new FreeCompanyChest
                    {
                        AetheryteId = 8,
                        ZoneId = 129,
                        Location = new Vector3("-200, 17.04425, 58.76245"),
                        NpcId = 2000470,
                        Name = "Company Chest"
                    }
                }
            },
            {
                Locations.UldahStepsOfNald,
                new INpc[]
                {
                    new FreeCompanyChest
                    {
                        AetheryteId = 9,
                        ZoneId = 130,
                        Location = new Vector3("-149.3096, 4.53186, -91.38635"),
                        NpcId = 2000470,
                        Name = "Company Chest"
                    }
                }
            }
        };

        public static readonly Dictionary<ShopItem, ShopItemInfo> ShopItemMap = new Dictionary<ShopItem, ShopItemInfo>
        {
#if RB_CN
			{
                ShopItem.CrpDelineation,
                new ShopItemInfo
                {
                    Index = 10 + (int) ShopItem.CrpDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12669 + (int) ShopItem.CrpDelineation,
                    Cost = 250,
                    Yield = 10
                }
            },
            {
                ShopItem.BsmDelineation,
                new ShopItemInfo
                {
                    Index = 10 + (int) ShopItem.BsmDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12669 + (int) ShopItem.BsmDelineation,
                    Cost = 250,
                    Yield = 10
                }
            },
            {
                ShopItem.ArmDelineation,
                new ShopItemInfo
                {
                    Index = 10 + (int) ShopItem.ArmDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12669 + (int) ShopItem.ArmDelineation,
                    Cost = 250,
                    Yield = 10
                }
            },
            {
                ShopItem.GsmDelineation,
                new ShopItemInfo
                {
                    Index = 10 + (int) ShopItem.GsmDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12669 + (int) ShopItem.GsmDelineation,
                    Cost = 250,
                    Yield = 10
                }
            },
            {
                ShopItem.LtwDelineation,
                new ShopItemInfo
                {
                    Index = 10 + (int) ShopItem.LtwDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12669 + (int) ShopItem.LtwDelineation,
                    Cost = 250,
                    Yield = 10
                }
            },
            {
                ShopItem.WvrDelineation,
                new ShopItemInfo
                {
                    Index = 10 + (int) ShopItem.WvrDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12669 + (int) ShopItem.WvrDelineation,
                    Cost = 250,
                    Yield = 10
                }
            },
            {
                ShopItem.AlcDelineation,
                new ShopItemInfo
                {
                    Index = 10 + (int) ShopItem.AlcDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12669 + (int) ShopItem.AlcDelineation,
                    Cost = 250,
                    Yield = 10
                }
            },
            {
                ShopItem.CulDelineation,
                new ShopItemInfo
                {
                    Index = 10 + (int) ShopItem.CulDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12669 + (int) ShopItem.CulDelineation,
                    Cost = 250,
                    Yield = 10
                }
            },
            {
                ShopItem.CommercialEngineeringManual,
                new ShopItemInfo
                {
                    Index = 10 + (int) ShopItem.CommercialEngineeringManual,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12669 + (int) ShopItem.CommercialEngineeringManual,
                    Cost = 45,
                    Yield = 1
                }
            },
            {
                ShopItem.RedCrafterToken,
                new ShopItemInfo
                {
                    Index = 0,
                    ShopType = ShopType.RedCrafter,
                    ItemId = 12838,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.RedGatherToken,
                new ShopItemInfo
                {
                    Index = 0,
                    ShopType = ShopType.RedGatherer,
                    ItemId = 12840,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.CommercialSurvivalManual,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.CommercialSurvivalManual,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12668,
                    Cost = 75,
                    Yield = 1
                }
            },
            {
                ShopItem.HiCordial,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.HiCordial,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12669,
                    Cost = 100,
                    Yield = 1
                }
            },
            {
                ShopItem.BlueToken,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.BlueToken,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12841,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.RedBalloon,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.RedBalloon,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12708,
                    Cost = 50,
                    Yield = 50
                }
            },
            {
                ShopItem.MagmaWorm,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.MagmaWorm,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12709,
                    Cost = 52,
                    Yield = 50
                }
            },
            {
                ShopItem.FiendWorm,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.FiendWorm,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12710,
                    Cost = 55,
                    Yield = 50
                }
            },
            {
                ShopItem.BruteLeech,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.BruteLeech,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12711,
                    Cost = 60,
                    Yield = 50
                }
            },
            {
                ShopItem.CraneFly,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.CraneFly,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12712,
                    Cost = 60,
                    Yield = 50
                }
            },
            {
                ShopItem.KukuruPowder,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.KukuruPowder,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12886,
                    Cost = 50,
                    Yield = 3
                }
            },
            {
                ShopItem.BouillonCube,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.BouillonCube,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12905,
                    Cost = 40,
                    Yield = 5
                }
            },
            {
                ShopItem.BeanSauce,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.BeanSauce,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12906,
                    Cost = 30,
                    Yield = 1
                }
            },
            {
                ShopItem.BeanPaste,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.BeanPaste,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12907,
                    Cost = 30,
                    Yield = 1
                }
            },
            {
                ShopItem.GoldenApple,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.GoldenApple,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 14142,
                    Cost = 50,
                    Yield = 5
                }
            },
            {
                ShopItem.SolsticeGarlic,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.SolsticeGarlic,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 14143,
                    Cost = 50,
                    Yield = 5
                }
            },
            {
                ShopItem.MatureOliveOil,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.MatureOliveOil,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 14144,
                    Cost = 50,
                    Yield = 5
                }
            },
            {
                ShopItem.LoaghtanFilet,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.LoaghtanFilet,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 14145,
                    Cost = 50,
                    Yield = 5
                }
            },
            {
                ShopItem.PowderedMermanHorn,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.PowderedMermanHorn,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 14937,
                    Cost = 60,
                    Yield = 5
                }
            }

#else            
            #region BlueCrafter
            {
                ShopItem.BlueCrafterToken,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.BlueCrafterToken,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12839,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.CommercialEngineeringManual,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.CommercialEngineeringManual,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12667,
                    Cost = 45,
                    Yield = 1
                }
            },
            {
                ShopItem.SweetCreamMilk,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.SweetCreamMilk,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 16734,
                    Cost = 16,
                    Yield = 1
                }
            },
            {
                ShopItem.StoneCheese,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.StoneCheese,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 16735,
                    Cost = 16,
                    Yield = 1
                }
            },
            {
                ShopItem.HeavensEgg,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.HeavensEgg,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 15652,
                    Cost = 16,
                    Yield = 1
                }
            },
            {
                ShopItem.CarbonFiber,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.CarbonFiber,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 5339,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.LoaghtanFilet,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.LoaghtanFilet,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 14145,
                    Cost = 10,
                    Yield = 1
                }
            },
            {
                ShopItem.GoldenApple,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.GoldenApple,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 14142,
                    Cost = 10,
                    Yield = 1
                }
            },
            {
                ShopItem.SolsticeGarlic,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.SolsticeGarlic,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 14143,
                    Cost = 10,
                    Yield = 1
                }
            },
            {
                ShopItem.MatureOliveOil,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.MatureOliveOil,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 14144,
                    Cost = 10,
                    Yield = 1
                }
            },
            {
                ShopItem.PowderedMermanHorn,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.PowderedMermanHorn,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 14937,
                    Cost = 12,
                    Yield = 1
                }
            },
            {
                ShopItem.BouillonCube,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.BouillonCube,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12905,
                    Cost = 8,
                    Yield = 1
                }
            },
            {
                ShopItem.BeanSauce,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.BeanSauce,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12906,
                    Cost = 30,
                    Yield = 1
                }
            },
            {
                ShopItem.BeanPaste,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.BeanPaste,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12907,
                    Cost = 30,
                    Yield = 1
                }
            },
            {
                ShopItem.KukuruPowder,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.KukuruPowder,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12886,
                    Cost = 16,
                    Yield = 1
                }
            },
            {
                ShopItem. AdeptsHat,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.AdeptsHat,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 11958,
                    Cost = 120,
                    Yield = 1
                }
            },
            {
                ShopItem.AdeptsGown,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.AdeptsGown,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 11963,
                    Cost = 270,
                    Yield = 1
                }
            },
            {
                ShopItem.AdeptsGloves,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.AdeptsGloves,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 11968,
                    Cost = 120,
                    Yield = 1
                }
            },
            {
                ShopItem.AdeptsHose,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.AdeptsHose,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 11976,
                    Cost = 105,
                    Yield = 1
                }
            },
            {
                ShopItem.AdeptsThighboots,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.AdeptsThighboots,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 11981,
                    Cost = 105,
                    Yield = 1
                }
            },
            {
                ShopItem.CrpDelineation,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.CrpDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12659,
                    Cost = 25,
                    Yield = 1
                }
            },
            {
                ShopItem.BsmDelineation,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.BsmDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12660,
                    Cost = 25,
                    Yield = 1
                }
            },
            {
                ShopItem.ArmDelineation,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.ArmDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12661,
                    Cost = 25,
                    Yield = 1
                }
            },
            {
                ShopItem.GsmDelineation,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.GsmDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12662,
                    Cost = 25,
                    Yield = 1
                }
            },
            {
                ShopItem.LtwDelineation,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.LtwDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12663,
                    Cost = 25,
                    Yield = 1
                }
            },
            {
                ShopItem.WvrDelineation,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.WvrDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12664,
                    Cost = 25,
                    Yield = 1
                }
            },
            {
                ShopItem.AlcDelineation,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.AlcDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12665,
                    Cost = 25,
                    Yield = 1
                }
            },
            {
                ShopItem.CulDelineation,
                new ShopItemInfo
                {
                    Index = (int) ShopItem.CulDelineation,
                    ShopType = ShopType.BlueCrafter,
                    ItemId = 12666,
                    Cost = 25,
                    Yield = 1
                }
            },
            #endregion

            #region RedCrafter
            {
                ShopItem.RedCrafterToken,
                new ShopItemInfo
                {
                    Index = 5 + (int) ShopItem.RedCrafterToken,
                    ShopType = ShopType.RedCrafter,
                    ItemId = 12838,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.GoblinCup,
                new ShopItemInfo
                {
                    Index = 5 + (int) ShopItem.GoblinCup,
                    ShopType = ShopType.RedCrafter,
                    ItemId = 14104,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.CompetenceIV,
                new ShopItemInfo
                {
                    Index = 5 + (int) ShopItem.CompetenceIV,
                    ShopType = ShopType.RedCrafter,
                    ItemId = 5702,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.CunningIV,
                new ShopItemInfo
                {
                    Index = 5 + (int) ShopItem.CunningIV,
                    ShopType = ShopType.RedCrafter,
                    ItemId = 5707,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.CommandIV,
                new ShopItemInfo
                {
                    Index = 5 + (int) ShopItem.CommandIV,
                    ShopType = ShopType.RedCrafter,
                    ItemId = 5712,
                    Cost = 50,
                    Yield = 1
                }
            },
            #endregion

            #region BlueGatherer
            {
                ShopItem.BlueGatherToken,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.BlueGatherToken,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12841,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.HiCordial,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.HiCordial,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12669,
                    Cost = 33,
                    Yield = 1
                }
            },
            {
                ShopItem.CommercialSurvivalManual,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.CommercialSurvivalManual,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12668,
                    Cost = 75,
                    Yield = 1
                }
            },
            {
                ShopItem.TrailblazersScarf,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.TrailblazersScarf,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 11986,
                    Cost = 200,
                    Yield = 1
                }
            },
            {
                ShopItem.TrailblazersVest,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.TrailblazersVest,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 11991,
                    Cost = 270,
                    Yield = 1
                }
            },
            {
                ShopItem.TrailblazersWristguards,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.TrailblazersWristguards,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 11996,
                    Cost = 150,
                    Yield = 1
                }
            },
            {
                ShopItem.TrailblazersSlops,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.TrailblazersSlops,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12004,
                    Cost = 100,
                    Yield = 1
                }
            },
            {
                ShopItem.TrailblazersShoes,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.TrailblazersShoes,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12009,
                    Cost = 100,
                    Yield = 1
                }
            },
            {
                ShopItem.BruteLeech,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.BruteLeech,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12711,
                    Cost = 1,
                    Yield = 1
                }
            },
            {
                ShopItem.CraneFly,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.CraneFly,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12712,
                    Cost = 1,
                    Yield = 1
                }
            },
            {
                ShopItem.FiendWorm,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.FiendWorm,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12710,
                    Cost = 1,
                    Yield = 1
                }
            },
            {
                ShopItem.MagmaWorm,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.MagmaWorm,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12709,
                    Cost = 1,
                    Yield = 1
                }
            },
            {
                ShopItem.RedBalloon,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.RedBalloon,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 12708,
                    Cost = 1,
                    Yield = 1
                }
            },
            {
                ShopItem.CrownTrout,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.CrownTrout,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13737,
                    Cost = 10,
                    Yield = 1
                }
            },
            {
                ShopItem.CrownTroutHQ,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.CrownTroutHQ,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13737,
                    Cost = 25,
                    Yield = 1
                }
            },
            {
                ShopItem.RetributionStaff,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.RetributionStaff,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13738,
                    Cost = 20,
                    Yield = 1
                }
            },
            {
                ShopItem.RetributionStaffHQ,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.RetributionStaffHQ,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13738,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.ThiefBetta,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.ThiefBetta,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13739,
                    Cost = 10,
                    Yield = 1
                }
            },
            {
                ShopItem.ThiefBettaHQ,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.ThiefBettaHQ,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13739,
                    Cost = 25,
                    Yield = 1
                }
            },
            {
                ShopItem.GoldsmithCrab,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.GoldsmithCrab,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13740,
                    Cost = 10,
                    Yield = 1
                }
            },
            {
                ShopItem.GoldsmithCrabHQ,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.GoldsmithCrabHQ,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13740,
                    Cost = 25,
                    Yield = 1
                }
            },
            {
                ShopItem.Pterodactyl,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.Pterodactyl,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13733,
                    Cost = 40,
                    Yield = 1
                }
            },
            {
                ShopItem.PterodactylHQ,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.PterodactylHQ,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13733,
                    Cost = 100,
                    Yield = 1
                }
            },
            {
                ShopItem.Eurhinosaur,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.Eurhinosaur,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13734,
                    Cost = 10,
                    Yield = 1
                }
            },
            {
                ShopItem.EurhinosaurHQ,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.EurhinosaurHQ,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13734,
                    Cost = 25,
                    Yield = 1
                }
            },
            {
                ShopItem.GemMarimo,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.GemMarimo,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13736,
                    Cost = 20,
                    Yield = 1
                }
            },
            {
                ShopItem.GemMarimoHQ,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.GemMarimoHQ,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13736,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.Sphalerite,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.Sphalerite,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13750,
                    Cost = 20,
                    Yield = 1
                }
            },
            {
                ShopItem.SphaleriteHQ,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.SphaleriteHQ,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13750,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.WindSilk,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.WindSilk,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13744,
                    Cost = 150,
                    Yield = 1
                }
            },
            {
                ShopItem.CloudCottonBoll,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.CloudCottonBoll,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13753,
                    Cost = 20,
                    Yield = 1
                }
            },
            {
                ShopItem.CloudCottonBollHQ,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.CloudCottonBollHQ,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13753,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.DinosaurLeather,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.DinosaurLeather,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13745,
                    Cost = 150,
                    Yield = 1
                }
            },
            {
                ShopItem.RoyalMistletoe,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.RoyalMistletoe,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13752,
                    Cost = 20,
                    Yield = 1
                }
            },
            {
                ShopItem.RoyalMistletoeHQ,
                new ShopItemInfo
                {
                    Index = 40 + (int) ShopItem.RoyalMistletoeHQ,
                    ShopType = ShopType.BlueGatherer,
                    ItemId = 13752,
                    Cost = 50,
                    Yield = 1
                }
            },
            #endregion

            #region RedGatherer
			{
                ShopItem.RedGatherToken,
                new ShopItemInfo
                {
                    Index = 45 + (int) ShopItem.RedGatherToken,
                    ShopType = ShopType.RedGatherer,
                    ItemId = 12840,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.GoblinDice,
                new ShopItemInfo
                {
                    Index = 45 + (int) ShopItem.GoblinDice,
                    ShopType = ShopType.RedGatherer,
                    ItemId = 14105,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.GuerdonIV,
                new ShopItemInfo
                {
                    Index = 45 + (int) ShopItem.GuerdonIV,
                    ShopType = ShopType.RedCrafter,
                    ItemId = 5687,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.GuileIV,
                new ShopItemInfo
                {
                    Index = 45 + (int) ShopItem.GuileIV,
                    ShopType = ShopType.RedCrafter,
                    ItemId = 5692,
                    Cost = 50,
                    Yield = 1
                }
            },
            {
                ShopItem.GraspIV,
                new ShopItemInfo
                {
                    Index = 45 + (int) ShopItem.GraspIV,
                    ShopType = ShopType.RedCrafter,
                    ItemId = 5697,
                    Cost = 50,
                    Yield = 1
                }
            }
            #endregion
#endif
		};

        public static IEnumerable<INpc> GetNpcsByLocation(Locations location)
        {
            IList<INpc> npcs;
            if (NpcMap.TryGetValue(location, out npcs))
            {
                return npcs;
            }

            return Enumerable.Empty<INpc>();
        }

        public static IEnumerable<T> GetNpcsByLocation<T>(Locations location) where T : INpc
        {
            return GetNpcsByLocation(location).OfType<T>();
        }
    }
}
