namespace ExBuddy.OrderBotTags.Behaviors.Objects
{
	using System.Collections.Generic;

	using Clio.Utilities;

	internal static class Data
	{
		public static readonly Dictionary<ShopItem, ShopItemInfo> ShopItemMap = new Dictionary<ShopItem, ShopItemInfo>
																					{
																						{
																							ShopItem.CrpDelineation,
																							new ShopItemInfo
																								{
																									Index = 8 + (int)ShopItem.CrpDelineation,
																									ShopType = ShopType.BlueCrafter,
																									ItemId = 12667 + (int)ShopItem.CrpDelineation,
																									Cost = 250,
																									Yield = 10
																								}
																						},
																						{
																							ShopItem.BsmDelineation,
																							new ShopItemInfo
																								{
																									Index = 8 + (int)ShopItem.BsmDelineation,
																									ShopType = ShopType.BlueCrafter,
																									ItemId = 12667 + (int)ShopItem.BsmDelineation,
																									Cost = 250,
																									Yield = 10
																								}
																						},
																						{
																							ShopItem.ArmDelineation,
																							new ShopItemInfo
																								{
																									Index = 8 + (int)ShopItem.ArmDelineation,
																									ShopType = ShopType.BlueCrafter,
																									ItemId = 12667 + (int)ShopItem.ArmDelineation,
																									Cost = 250,
																									Yield = 10
																								}
																						},
																						{
																							ShopItem.GsmDelineation,
																							new ShopItemInfo
																								{
																									Index = 8 + (int)ShopItem.GsmDelineation,
																									ShopType = ShopType.BlueCrafter,
																									ItemId = 12667 + (int)ShopItem.GsmDelineation,
																									Cost = 250,
																									Yield = 10
																								}
																						},
																						{
																							ShopItem.LtwDelineation,
																							new ShopItemInfo
																								{
																									Index = 8 + (int)ShopItem.LtwDelineation,
																									ShopType = ShopType.BlueCrafter,
																									ItemId = 12667 + (int)ShopItem.LtwDelineation,
																									Cost = 250,
																									Yield = 10
																								}
																						},
																						{
																							ShopItem.WvrDelineation,
																							new ShopItemInfo
																								{
																									Index = 8 + (int)ShopItem.WvrDelineation,
																									ShopType = ShopType.BlueCrafter,
																									ItemId = 12667 + (int)ShopItem.WvrDelineation,
																									Cost = 250,
																									Yield = 10
																								}
																						},
																						{
																							ShopItem.AlcDelineation,
																							new ShopItemInfo
																								{
																									Index = 8 + (int)ShopItem.AlcDelineation,
																									ShopType = ShopType.BlueCrafter,
																									ItemId = 12667 + (int)ShopItem.AlcDelineation,
																									Cost = 250,
																									Yield = 10
																								}
																						},
																						{
																							ShopItem.CulDelineation,
																							new ShopItemInfo
																								{
																									Index = 8 + (int)ShopItem.CulDelineation,
																									ShopType = ShopType.BlueCrafter,
																									ItemId = 12667 + (int)ShopItem.CulDelineation,
																									Cost = 250,
																									Yield = 10
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
																							ShopItem.HiCordial,
																							new ShopItemInfo
																								{
																									Index = (int)ShopItem.HiCordial,
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
																									Index = (int)ShopItem.BlueToken,
																									ShopType = ShopType.BlueGatherer,
																									ItemId = 12841,
																									Cost = 250,
																									Yield = 5
																								}
																						},
																						{
																							ShopItem.BruteLeech,
																							new ShopItemInfo
																								{
																									Index = (int)ShopItem.BruteLeech,
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
																									Index = (int)ShopItem.CraneFly,
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
																									Index = (int)ShopItem.KukuruPowder,
																									ShopType = ShopType.BlueCrafter,
																									ItemId = 12886,
																									Cost = 50,
																									Yield = 1
																								}
																						},
																						{
																							ShopItem.BouillonCube,
																							new ShopItemInfo
																								{
																									Index = (int)ShopItem.BouillonCube,
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
																									Index = (int)ShopItem.BeanSauce,
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
																									Index = (int)ShopItem.BeanPaste,
																									ShopType = ShopType.BlueCrafter,
																									ItemId = 12907,
																									Cost = 30,
																									Yield = 1
																								}
																						},
																					};

		public static readonly Dictionary<Locations, LocationData> LocationMap = new Dictionary<Locations, LocationData>
																					{
																						{
																							Locations.MorDhona,
																							new LocationData
																								{
																									AetheryteId = 24,
																									ZoneId = 156,
																									NpcId = 1013396,
																									NpcLocation = new Vector3("50.33948, 31.13618, -737.4532"),
																									ShopNpcId = 1013397,
																									ShopNpcLocation = new Vector3("47.34875, 31.15659, -737.4838")
																								}
																						},
																						{
																							Locations.Idyllshire,
																							new LocationData
																								{
																									AetheryteId = 75,
																									ZoneId = 478,
																									NpcId = 1012300,
																									NpcLocation = new Vector3("-15.64056, 211, 0.1677856"),
																									ShopNpcId = 1012301,
																									ShopNpcLocation = new Vector3("-17.38013, 211, -1.66333")
																								}
																						}
																					};
	}
}