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
					Index = 1 - (int) ShopItem.RedCrafterToken,
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
					Index = -1 + (int) ShopItem.GoblinCup,
					ShopType = ShopType.RedCrafter,
					ItemId = 14104,
					Cost = 50,
					Yield = 1
				}
			},
			{
				ShopItem.RedGatherToken,
				new ShopItemInfo
				{
					Index = 2 - (int) ShopItem.RedGatherToken,
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
					Index = -2 + (int) ShopItem.GoblinDice,
					ShopType = ShopType.RedGatherer,
					ItemId = 14105,
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
				ShopItem.BlueGatherToken,
				new ShopItemInfo
				{
					Index = (int) ShopItem.BlueGatherToken,
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
			},
				{
				ShopItem.HeavensEgg,
				new ShopItemInfo
				{
					Index = (int) ShopItem.HeavensEgg,
					ShopType = ShopType.BlueCrafter,
					ItemId = 15652,
					Cost = 50,
					Yield = 3
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
			}
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
