
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
	using ExBuddy.Helpers;
	using ExBuddy.OrderBotTags.Behaviors.Objects;
	using ExBuddy.OrderBotTags.Objects;
	using ExBuddy.Providers;
	using ExBuddy.RemoteWindows;

	using ff14bot;
	using ff14bot.Behavior;
	using ff14bot.Enums;
	using ff14bot.Helpers;
	using ff14bot.Managers;
	using ff14bot.Navigation;
	using ff14bot.NeoProfiles;
	using ff14bot.RemoteWindows;

	using TreeSharp;

	[LoggerName("TurnInCollectables")]
	[XmlElement("TurnInCollectables")]
	public class TurnInCollectablesTag : ExProfileBehavior
	{
		private bool isDone;

		private bool turnedItemsIn;

		private BagSlot item;

		private uint index;

		private LocationData locationData;

		[DefaultValue(Locations.Idyllshire)]
		[XmlAttribute("Location")]
		public Locations Location { get; set; }

		[XmlAttribute("ForcePurchase")]
		public bool ForcePurchase { get; set; }

		[Clio.XmlEngine.XmlElement("Collectables")]
		public List<CollectableTurnIn> Collectables { get; set; }

		[Clio.XmlEngine.XmlElement("ShopPurchases")]
		public List<ShopPurchase> ShopPurchases { get; set; }

		protected override Color Info
		{
			get
			{
				return Colors.MediumSpringGreen;
			}
		}

		protected override void OnStart()
		{
			locationData = Data.LocationMap[Location];
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

			if (MasterPieceSupply.IsOpen)
			{
				MasterPieceSupply.Close();
			}

			if (ShopExchangeCurrency.IsOpen)
			{
				ShopExchangeCurrency.Close();
			}

			if (SelectIconString.IsOpen)
			{
				SelectIconString.ClickSlot(uint.MaxValue);
			}
		}

		protected override void OnResetCachedDone()
		{
			StatusText = string.Empty;
			isDone = false;
			turnedItemsIn = false;
			item = null;
			index = 0;
		}

		protected override Composite CreateBehavior()
		{
			return new ActionRunCoroutine(ctx => Main());
		}

		public override bool IsDone
		{
			get
			{
				return isDone;
			}
		}

		private async Task<bool> Main()
		{
			await CommonTasks.HandleLoading();

			return await ResolveItem() || HandleDeath() || await Behaviors.TeleportTo(locationData) || await MoveToNpc()
					|| await InteractWithNpc() || await ResolveIndex() || await HandOver() || await HandleSkipPurchase()
					|| await MoveToShopNpc() || await PurchaseItems();
		}

		private async Task<bool> HandleSkipPurchase()
		{
			if (ShopPurchases == null || ShopPurchases.Count == 0 || ShopPurchases.All(s => !ShouldPurchaseItem(s)))
			{
				Logger.Info("No items to purchase");
				isDone = true;
				return true;
			}

			return false;
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

		private async Task<bool> MoveToShopNpc()
		{
			if (Me.Location.Distance(locationData.ShopNpcLocation) <= 4)
			{
				// we are already there, continue
				return false;
			}

			await
				Behaviors.MoveTo(
					locationData.ShopNpcLocation,
					radius: 4.0f,
					name: Location + " ShopNpcId: " + locationData.ShopNpcId);

			Navigator.Stop();

			return false;
		}

		private async Task<bool> PurchaseItems()
		{
			if (Me.Location.Distance(locationData.ShopNpcLocation) > 4)
			{
				// too far away, should go back to MoveToNpc
				return true;
			}

			StatusText = "Purchasing items";

			var itemsToPurchase = ShopPurchases.Where(ShouldPurchaseItem).ToArray();
			var npc = GameObjectManager.GetObjectByNPCId(locationData.ShopNpcId);
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
					Logger.Error("Timeout targeting npc.");
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
					Logger.Error("Timeout interacting with npc.");
					isDone = true;
					return true;
				}

				if (Location == Locations.MorDhona
					&& (purchaseItemInfo.ShopType == ShopType.RedCrafter || purchaseItemInfo.ShopType == ShopType.RedGatherer))
				{
					Logger.Warn("Unable to purchase item {0} in MorDhona, set location to Idyllshire.", purchaseItemData.EnglishName);
					continue;
				}

				ticks = 0;
				while (SelectIconString.IsOpen && ticks++ < 5 && Behaviors.ShouldContinue)
				{
					if (Location == Locations.MorDhona)
					{
						// Blue crafter = 0, Blue gather = 1
						SelectIconString.ClickSlot((uint)purchaseItemInfo.ShopType / 2);
					}
					else
					{
						SelectIconString.ClickSlot((uint)purchaseItemInfo.ShopType);
					}

					await shopExchangeCurrency.Refresh(5000);
				}

				if (ticks > 5 || !shopExchangeCurrency.IsValid)
				{
					Logger.Error("Timeout interacting with npc.");
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
						&& (scripsLeft = Memory.Scrips.GetRemainingScripsByShopType(purchaseItemInfo.ShopType)) >= purchaseItemInfo.Cost
						&& Behaviors.ShouldContinue)
				{
					if (!await shopExchangeCurrency.PurchaseItem(purchaseItemInfo.Index, 20))
					{
						Logger.Error("Timeout during purchase of {0}", purchaseItemData.EnglishName);
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
						"Purchased item {0} for {1} {2} scrips at {3} ET; Remaining Scrips: {4}",
						purchaseItemData.EnglishName,
						purchaseItemInfo.Cost,
						purchaseItemInfo.ShopType,
						WorldManager.EorzaTime,
						scripsLeft);

					await Coroutine.Yield();
				}

				await Coroutine.Sleep(1000);
			}

			Logger.Info("Purchases complete.");
			SelectYesno.ClickNo();
			if (SelectIconString.IsOpen)
			{
				SelectIconString.ClickSlot(uint.MaxValue);
			}

			await shopExchangeCurrency.CloseInstance();
			isDone = true;
			return true;
		}

		private bool HandleDeath()
		{
			if (Me.IsDead && Poi.Current.Type != PoiType.Death)
			{
				Poi.Current = new Poi(Me, PoiType.Death);
				return true;
			}

			return false;
		}

		private async Task<bool> HandOver()
		{
			var masterpieceSupply = new MasterPieceSupply();
			if (!masterpieceSupply.IsValid && !await masterpieceSupply.Refresh(2000))
			{
				return false;
			}

			if (item == null || item.Item == null)
			{
				SelectYesno.ClickNo();
				await masterpieceSupply.CloseInstanceGently(15);

				return false;
			}

			StatusText = "Turning in items";

			var itemName = item.Item.EnglishName;

			if (!await masterpieceSupply.TurnInAndHandOver(index, item))
			{
				Logger.Error("An error has occured while turning in the item");
				Blacklist.Add(
					(uint)item.Pointer.ToInt32(),
					BlacklistFlags.Loot,
					TimeSpan.FromMinutes(3),
					"Don't turn in this item for 3 minutes");
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

			Logger.Info("Turned in {0} at {1} ET", itemName, WorldManager.EorzaTime);

			turnedItemsIn = true;

			index = 0;
			if (!await Coroutine.Wait(1000, () => item == null))
			{
				item = null;
			}

			return true;
		}

		private async Task<bool> MoveToNpc()
		{
			if (item == null || item.Item == null)
			{
				return false;
			}

			if (Me.Location.Distance(locationData.NpcLocation) <= 4)
			{
				// we are already there, continue
				return false;
			}

			StatusText = "Moving to Npc -> " + locationData.NpcId;

			await Behaviors.MoveTo(locationData.NpcLocation, radius: 4.0f, name: Location + " NpcId: " + locationData.NpcId);

			Navigator.Stop();

			return false;
		}

		private async Task<bool> InteractWithNpc()
		{
			if (item == null || item.Item == null)
			{
				return false;
			}

			if (Me.Location.Distance(locationData.NpcLocation) > 4)
			{
				// too far away, should go back to MoveToNpc
				return true;
			}

			if (GameObjectManager.Target != null && MasterPieceSupply.IsOpen)
			{
				// already met conditions
				return false;
			}

			var npc = GameObjectManager.GetObjectByNPCId(locationData.NpcId);
			npc.Target();
			npc.Interact();

			StatusText = "Interacting with Npc -> " + npc.EnglishName;

			return false;
		}

		private async Task<bool> ResolveIndex()
		{
			if (item == null || item.Item == null)
			{
				return false;
			}

			var provider = MasterPieceSupplyDataProvider.Instance;

			if (provider.IsValid)
			{
				var i = provider.GetIndexByItemName(item.EnglishName);
				if (i.HasValue)
				{
					index = i.Value;
					return false;
				}
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
				classIndex = MasterPieceSupply.GetClassIndex((ClassJobType)item.Item.RepairClass);
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
					Logger.Error("Error, could not resolve class type for item: " + item.Item.EnglishName);
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
					itemLevel = itemLevel < 120 ? (byte)0 : (byte)((itemLevel - 121) / 3);
					break;
			}

			int indexOffset;

			if (classIndex >= 8)
			{
				if (itemLevel >= 10)
				{
					indexOffset = (8 + Math.Abs((int)classIndex - 10) * 2);
				}
				else
				{
					indexOffset = 62 + Math.Abs((int)classIndex - 10) * 6;
					indexOffset += Math.Abs(itemLevel - 10) / 2;
				}
			}
			else
			{
				if (itemLevel >= 10)
				{
					indexOffset = Math.Abs((int)classIndex - 7);
				}
				else
				{
					indexOffset = 14 + Math.Abs((int)classIndex - 7) * 6;
					indexOffset += Math.Abs(itemLevel - 10) / 2;
				}
			}

			index = (uint)indexOffset;

			return false;
		}

		private async Task<bool> ResolveItem()
		{
			if (item != null)
			{
				return false;
			}

			var slots =
				InventoryManager.FilledSlots.Where(i => !Blacklist.Contains((uint)i.Pointer.ToInt32(), BlacklistFlags.Loot))
					.ToArray();

			if (Collectables == null)
			{
				item = slots.FirstOrDefault(i => i.Collectability > 0);
			}
			else
			{
				foreach (var collectable in Collectables)
				{
					item =
						slots.FirstOrDefault(
							i =>
							i.Collectability >= collectable.Value && i.Collectability <= collectable.MaxValueForTurnIn
							&& string.Equals(collectable.Name, i.EnglishName, StringComparison.InvariantCultureIgnoreCase));

					if (item != null)
					{
						break;
					}
				}
			}

			if (item != null && item.Item != null)
			{
				Logger.Verbose("Attempting to turn in item {0} -> 0x{1}", item.EnglishName, item.Pointer.ToString("X8"));
				return false;
			}

			if ((turnedItemsIn || ForcePurchase) && !await HandleSkipPurchase())
			{
				return false;
			}

			isDone = true;
			return true;
		}
	}
}