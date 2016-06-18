
#pragma warning disable 1998

namespace ExBuddy.OrderBotTags.Behaviors
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Media;
	using Buddy.Coroutines;
	using Clio.XmlEngine;
	using ExBuddy.Attributes;
	using ExBuddy.Data;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ExBuddy.OrderBotTags.Behaviors.Objects;
	using ExBuddy.OrderBotTags.Objects;
	using ExBuddy.Windows;
	using ff14bot;
	using ff14bot.Behavior;
	using ff14bot.Enums;
	using ff14bot.Helpers;
	using ff14bot.Managers;
	using ff14bot.Navigation;
	using ff14bot.NeoProfiles;
	using ff14bot.RemoteWindows;

	[LoggerName("ExTurnInCollectables")]
	[XmlElement("ExTurnInCollectables")]
	[XmlElement("TurnInCollectables")]
	public class ExTurnInCollectablesTag : ExProfileBehavior
	{
		private uint index;

		private BagSlot item;

		private INpc masterPieceSupplyNpc;

		private INpc shopExchangeCurrencyNpc;

		private bool turnedItemsIn;

		[XmlElement("Collectables")]
		public List<CollectableTurnIn> Collectables { get; set; }

		[XmlAttribute("ForcePurchase")]
		public bool ForcePurchase { get; set; }

		[DefaultValue(Locations.Idyllshire)]
		[XmlAttribute("Location")]
		public Locations Location { get; set; }

		[XmlElement("ShopPurchases")]
		public List<ShopPurchase> ShopPurchases { get; set; }

		protected override Color Info
		{
			get { return Colors.MediumSpringGreen; }
		}

		protected override void DoReset()
		{
			turnedItemsIn = false;
			item = null;
			index = 0;
		}

		protected override async Task<bool> Main()
		{
			await CommonTasks.HandleLoading();

			return await ResolveItem() || HandleDeath() || await masterPieceSupplyNpc.TeleportTo() || await MoveToNpc()
			       || await InteractWithNpc() || await ResolveIndex() || await HandOver() || await HandleSkipPurchase()
			       || await MoveToShopNpc() || await PurchaseItems();
		}

		protected override void OnDone()
		{
			if (SelectYesno.IsOpen)
			{
				SelectYesno.ClickNo();
			}

			if (Request.IsOpen)
			{
				Request.Cancel();
			}

			if (Window<MasterPieceSupply>.IsOpen)
			{
				MasterPieceSupply.Close();
			}

			if (Window<ShopExchangeCurrency>.IsOpen)
			{
				ShopExchangeCurrency.Close();
			}

			if (SelectIconString.IsOpen)
			{
				SelectIconString.ClickSlot(uint.MaxValue);
			}
		}

		protected override void OnStart()
		{
			var npcs = Data.GetNpcsByLocation(Location).ToArray();

			masterPieceSupplyNpc = npcs.OfType<GameObjects.Npcs.MasterPieceSupply>().FirstOrDefault();
			shopExchangeCurrencyNpc = npcs.OfType<GameObjects.Npcs.ShopExchangeCurrency>().FirstOrDefault();

			ShopPurchases = ShopPurchases ?? new List<ShopPurchase>();
		}

		private bool HandleDeath()
		{
			if (ExProfileBehavior.Me.IsDead && Poi.Current.Type != PoiType.Death)
			{
				Poi.Current = new Poi(ExProfileBehavior.Me, PoiType.Death);
				return true;
			}

			return false;
		}

		private async Task<bool> HandleSkipPurchase()
		{
			if (ShopPurchases == null || ShopPurchases.Count == 0 || ShopPurchases.All(s => !ShouldPurchaseItem(s)))
			{
				Logger.Info(Localization.Localization.ExTurnInCollectable_NoItemToPurchase);
				LogInventoryForPurchaseInfos();
				LogScripsRemainingForPurchaseInfos();
				return isDone = true;
			}

			return false;
		}

		private async Task<bool> HandOver()
		{
			var masterpieceSupply = new MasterPieceSupply();
			if (!masterpieceSupply.IsValid && !await masterpieceSupply.Refresh(5000))
			{
				return false;
			}

			if (item == null || item.Item == null)
			{
				SelectYesno.ClickNo();
				await masterpieceSupply.CloseInstanceGently(15);

				return false;
			}

			StatusText = Localization.Localization.ExTurnInCollectable_TurnIn;

			var itemName = item.Item.CurrentLocaleName;

			if (!await masterpieceSupply.TurnInAndHandOver(index, item))
			{
				Logger.Error(Localization.Localization.ExTurnInCollectable_TurnInError);
				Blacklist.Add(
					(uint) item.Pointer.ToInt32(),
					BlacklistFlags.Loot,
					TimeSpan.FromMinutes(3),
                    Localization.Localization.ExTurnInCollectable_TurnInBlackList);
				item = null;
				index = 0;

				if (SelectYesno.IsOpen)
				{
					SelectYesno.ClickNo();
					await Coroutine.Sleep(200);
				}

				if (Request.IsOpen)
				{
					Request.Cancel();
					await Coroutine.Sleep(200);
				}

				return true;
			}

			Logger.Info(Localization.Localization.ExTurnInCollectable_TurnInSuccessful, itemName, WorldManager.EorzaTime);

			turnedItemsIn = true;

			index = 0;
			if (!await Coroutine.Wait(1000, () => item == null))
			{
				item = null;
			}

			return true;
		}

		private async Task<bool> InteractWithNpc()
		{
			if (item == null || item.Item == null)
			{
				return false;
			}

			if (ExProfileBehavior.Me.Location.Distance(masterPieceSupplyNpc.Location) > 4)
			{
				// too far away, should go back to MoveToNpc
				return true;
			}

			if (GameObjectManager.Target != null && Window<MasterPieceSupply>.IsOpen)
			{
				// already met conditions
				return false;
			}

			await masterPieceSupplyNpc.Interact(4);

			StatusText = Localization.Localization.ExTurnInCollectable_NpcInteract + masterPieceSupplyNpc.NpcId;
			await Coroutine.Yield();

			return false;
		}

		private void LogInventoryForPurchaseInfos()
		{
			foreach (var purchaseItem in ShopPurchases)
			{
				var purchaseItemInfo = Data.ShopItemMap[purchaseItem.ShopItem];
				var purchaseItemData = purchaseItemInfo.ItemData;

				Logger.Info(Localization.Localization.ExTurnInCollectable_ShopPurchase, purchaseItemData.EnglishName, purchaseItemData.ItemCount());
			}
		}

		private void LogScripsRemainingForPurchaseInfos()
		{
			var result = ShopPurchases.Select(sp => Data.ShopItemMap[sp.ShopItem].ShopType).Distinct().ToArray();

			foreach (var shopType in result)
			{
				Logger.Info(
                    Localization.Localization.ExTurnInCollectable_ScripsRemaining,
					shopType,
					Memory.Scrips.GetRemainingScripsByShopType(shopType));
			}
		}

		private async Task<bool> MoveToNpc()
		{
			if (item == null || item.Item == null)
			{
				return false;
			}

			if (ExProfileBehavior.Me.Location.Distance(masterPieceSupplyNpc.Location) <= 4)
			{
				// we are already there, continue
				return false;
			}

			StatusText = Localization.Localization.ExTurnInCollectable_Move + masterPieceSupplyNpc.NpcId;

			await masterPieceSupplyNpc.Location.MoveTo(radius: 3.9f, name: Location + " NpcId: " + masterPieceSupplyNpc.NpcId);

			return false;
		}

		private async Task<bool> MoveToShopNpc()
		{
			var masterPieceSupply = new MasterPieceSupply();
			if (await masterPieceSupply.Refresh(2000))
			{
				await masterPieceSupply.CloseInstanceGently();
			}

			if (ExProfileBehavior.Me.Location.Distance(shopExchangeCurrencyNpc.Location) <= 4)
			{
				// we are already there, continue
				return false;
			}

			await
				shopExchangeCurrencyNpc.Location.MoveTo(
					radius: 3.9f,
					name: Location + " ShopNpcId: " + shopExchangeCurrencyNpc.NpcId);

			Navigator.Stop();

			return false;
		}

		private async Task<bool> PurchaseItems()
		{
			if (ExProfileBehavior.Me.Location.Distance(shopExchangeCurrencyNpc.Location) > 4)
			{
				// too far away, should go back to MoveToNpc
				return true;
			}

			StatusText = Localization.Localization.ExTurnInCollectable_Purchase;

			var itemsToPurchase = ShopPurchases.Where(ShouldPurchaseItem).ToArray();
			var npc = GameObjectManager.GetObjectByNPCId(shopExchangeCurrencyNpc.NpcId);
			var shopType = ShopType.BlueGatherer;
			var shopExchangeCurrency = new ShopExchangeCurrency();
			foreach (var purchaseItem in itemsToPurchase)
			{
				var purchaseItemInfo = Data.ShopItemMap[purchaseItem.ShopItem];
				var purchaseItemData = purchaseItemInfo.ItemData;

				if (shopType != purchaseItemInfo.ShopType && shopExchangeCurrency.IsValid)
				{
					await shopExchangeCurrency.CloseInstanceGently();
				}

				shopType = purchaseItemInfo.ShopType;

				// target
				var ticks = 0;
				while (Core.Target == null && !shopExchangeCurrency.IsValid && ticks++ < 10 && Behaviors.ShouldContinue)
				{
					npc.Target();
					await Coroutine.Wait(1000, () => Core.Target != null);
				}

				// check for timeout
				if (ticks > 10)
				{
					Logger.Error(Localization.Localization.ExTurnInCollectable_TargetingTimeout);
					isDone = true;
					return true;
				}

				// interact
				ticks = 0;
				while (!SelectIconString.IsOpen && !shopExchangeCurrency.IsValid && ticks++ < 10 && Behaviors.ShouldContinue)
				{
					npc.Interact();
					await Coroutine.Wait(1000, () => SelectIconString.IsOpen);
				}

				// check for timeout
				if (ticks > 10)
				{
					Logger.Error(Localization.Localization.ExTurnInCollectable_InteractingTimeout);
					isDone = true;
					return true;
				}

				if (Location == Locations.MorDhona
				    && (purchaseItemInfo.ShopType == ShopType.RedCrafter || purchaseItemInfo.ShopType == ShopType.RedGatherer))
				{
					Logger.Warn(Localization.Localization.ExTurnInCollectable_FailedPurchaseMorDhona, purchaseItemData.EnglishName);
					continue;
				}

				ticks = 0;
				while (SelectIconString.IsOpen && ticks++ < 5 && Behaviors.ShouldContinue)
				{
					if (Location == Locations.MorDhona)
					{
						// Blue crafter = 0, Blue gather = 1
						SelectIconString.ClickSlot((uint) purchaseItemInfo.ShopType/2);
					}
					else
					{
						SelectIconString.ClickSlot((uint) purchaseItemInfo.ShopType);
					}

					await shopExchangeCurrency.Refresh(5000);
				}

				if (ticks > 5 || !shopExchangeCurrency.IsValid)
				{
					Logger.Error(Localization.Localization.ExTurnInCollectable_InteractingTimeout);
					if (SelectIconString.IsOpen)
					{
						SelectIconString.ClickSlot(uint.MaxValue);
					}

					isDone = true;
					return true;
				}

				await Coroutine.Sleep(600);
				int scripsLeft;
				while (purchaseItemData.ItemCount() < purchaseItem.MaxCount
				       &&
				       (scripsLeft = Memory.Scrips.GetRemainingScripsByShopType(purchaseItemInfo.ShopType)) >= purchaseItemInfo.Cost
				       && Behaviors.ShouldContinue)
				{
					if (!await shopExchangeCurrency.PurchaseItem(purchaseItemInfo.Index, 20))
					{
						Logger.Error(Localization.Localization.ExTurnInCollectable_PurchaseTimeout, purchaseItemData.EnglishName);
						await shopExchangeCurrency.CloseInstance();
						isDone = true;
						return true;
					}

					// wait until scrips changed
					var left = scripsLeft;
					await
						Coroutine.Wait(
							5000,
							() => (scripsLeft = Memory.Scrips.GetRemainingScripsByShopType(purchaseItemInfo.ShopType)) != left);

					Logger.Info(
                        Localization.Localization.ExTurnInCollectable_Purchased,
						purchaseItemData.EnglishName,
						purchaseItemInfo.Cost,
						purchaseItemInfo.ShopType,
						WorldManager.EorzaTime,
						scripsLeft);

					await Coroutine.Yield();
				}

				await Coroutine.Sleep(1000);
			}

			Logger.Info(Localization.Localization.ExTurnInCollectable_PurchaseComplete);
			SelectYesno.ClickNo();
			if (SelectIconString.IsOpen)
			{
				SelectIconString.ClickSlot(uint.MaxValue);
			}

			await shopExchangeCurrency.CloseInstance();
			isDone = true;
			return true;
		}

		private async Task<bool> ResolveIndex()
		{
			if (item == null || item.Item == null)
			{
				return false;
			}

			var provider = SqlData.Instance;

			var i = provider.GetIndexByItemId(item.RawItemId);
			if (i.HasValue)
			{
				index = i.Value;
				return false;
			}

			switch (item.RawItemId)
			{
				case 12774U:
				case 12828U:
					index = 9; // Tiny Axotl + Thunderbolt Eel
					return false;
				case 12900U: // Chysahl Greens
					index = 11;
					return false;
				case 12538U: // Adamantite Ore
					index = 13;
					return false;
				case 12804U: // Illuminati Perch
					index = 62;
					return false;
			}

			// No perfect algorithm for this, but will attempt.  Going to have to read the data from the window.
			// for some reason, seafood has a repair class of cul... go figure.
			var classIndex = uint.MaxValue;
			if (item.Item.RepairClass > 0 && item.Item.EquipmentCatagory != ItemUiCategory.Seafood)
			{
				classIndex = MasterPieceSupply.GetClassIndex((ClassJobType) item.Item.RepairClass);
			}
			else
			{
				switch (item.Item.EquipmentCatagory)
				{
					case ItemUiCategory.Seafood:
						classIndex = MasterPieceSupply.GetClassIndex(ClassJobType.Fisher);
						break;
					case ItemUiCategory.Stone:
					case ItemUiCategory.Metal:
					case ItemUiCategory.Bone:
						classIndex = MasterPieceSupply.GetClassIndex(ClassJobType.Miner);
						break;
					case ItemUiCategory.Reagent:
					case ItemUiCategory.Ingredient:
					case ItemUiCategory.Lumber:
						classIndex = MasterPieceSupply.GetClassIndex(ClassJobType.Botanist);
						break;
				}

				if (classIndex == uint.MaxValue)
				{
					Logger.Error(Localization.Localization.ExTurnInCollectable_ErrorClassType + item.Item.EnglishName);
					isDone = true;
					return true;
				}
			}

			var itemLevel = item.Item.ItemLevel;

			switch (itemLevel)
			{
				case 80:
					itemLevel = 0;
					break;
				case 120:
					itemLevel = 1;
					break;
				case 125:
					itemLevel = 2;
					break;
				case 150:
					itemLevel = 10;
					break;
				case 160:
					itemLevel = 11;
					break;
				case 180:
					itemLevel = 12;
					break;
				default:
					itemLevel = itemLevel < 120 ? (byte) 0 : (byte) ((itemLevel - 121)/3);
					break;
			}

			int indexOffset;

			if (classIndex >= 8)
			{
				if (itemLevel >= 10)
				{
					indexOffset = (8 + Math.Abs((int) classIndex - 10)*2);
				}
				else
				{
					indexOffset = 62 + Math.Abs((int) classIndex - 10)*6;
					indexOffset += Math.Abs(itemLevel - 10)/2;
				}
			}
			else
			{
				if (itemLevel >= 10)
				{
					indexOffset = Math.Abs((int) classIndex - 7);
				}
				else
				{
					indexOffset = 14 + Math.Abs((int) classIndex - 7)*6;
					indexOffset += Math.Abs(itemLevel - 10)/2;
				}
			}

			index = (uint) indexOffset;

			return false;
		}

		private async Task<bool> ResolveItem()
		{
			if (item != null)
			{
				return false;
			}

			var slots =
				InventoryManager.FilledInventoryAndArmory.Where(
					i => !Blacklist.Contains((uint) i.Pointer.ToInt32(), BlacklistFlags.Loot)).ToArray();

			if (Collectables == null)
			{
				item = slots.FirstOrDefault(i => i.Collectability > 0);
			}
			else
			{
				foreach (var collectable in Collectables)
				{
					var bagslots = slots.Where(i =>
						i.Collectability >= collectable.Value && i.Collectability <= collectable.MaxValueForTurnIn).ToArray();

					if (collectable.Id > 0)
					{
						item =
							bagslots.FirstOrDefault(i => i.RawItemId == collectable.Id);
					}

					item = item ??
						bagslots.FirstOrDefault(
							i => string.Equals(collectable.LocalName, i.Name, StringComparison.InvariantCultureIgnoreCase)) ??
						bagslots.FirstOrDefault(
							i => string.Equals(collectable.Name, i.EnglishName, StringComparison.InvariantCultureIgnoreCase));

					if (item != null)
					{
						break;
					}
				}
			}

			if (item != null && item.Item != null)
			{
				Logger.Verbose(Localization.Localization.ExTurnInCollectable_AttemptingTurnin, item.EnglishName, item.Pointer.ToString("X8"));
				return false;
			}

			if ((turnedItemsIn || ForcePurchase) && !await HandleSkipPurchase())
			{
				return false;
			}

			if (SelectYesno.IsOpen)
			{
				SelectYesno.ClickNo();
			}

			if (Request.IsOpen)
			{
				Request.Cancel();
			}

			var masterpieceSupply = new MasterPieceSupply();
			if (masterpieceSupply.IsValid)
			{
				await masterpieceSupply.CloseInstanceGently();
			}

			var shopExchangeCurrency = new ShopExchangeCurrency();
			if (shopExchangeCurrency.IsValid)
			{
				await shopExchangeCurrency.CloseInstanceGently();
			}

			if (SelectIconString.IsOpen)
			{
				SelectIconString.ClickSlot(uint.MaxValue);
			}

			return isDone = true;
		}

		private bool ShouldPurchaseItem(ShopPurchase shopPurchase)
		{
			var info = Data.ShopItemMap[shopPurchase.ShopItem];

			var itemData = info.ItemData;

			var itemCount = itemData.ItemCount();
			// check inventory count
			if (itemCount >= shopPurchase.MaxCount)
			{
				return false;
			}

			if (ConditionParser.FreeItemSlots() == 0 && itemCount == 0)
			{
				return false;
			}

			// check cost
			switch (info.ShopType)
			{
				case ShopType.BlueCrafter:
					if (Memory.Scrips.BlueCrafter < info.Cost)
					{
						return false;
					}
					break;
				case ShopType.RedCrafter:
					if (Memory.Scrips.RedCrafter < info.Cost)
					{
						return false;
					}
					break;
				case ShopType.BlueGatherer:
					if (Memory.Scrips.BlueGatherer < info.Cost)
					{
						return false;
					}
					break;
				case ShopType.RedGatherer:
					if (Memory.Scrips.RedGatherer < info.Cost)
					{
						return false;
					}
					break;
			}

			return true;
		}
	}
}