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

    using Clio.Utilities;
    using Clio.XmlEngine;

    using ExBuddy.Enums;
    using ExBuddy.Helpers;
    using ExBuddy.OrderBotTags.Behaviors.Objects;
    using ExBuddy.OrderBotTags.Objects;

    using ff14bot;
    using ff14bot.Behavior;
    using ff14bot.Enums;
    using ff14bot.Helpers;
    using ff14bot.Managers;
    using ff14bot.Navigation;
    using ff14bot.NeoProfiles;
    using ff14bot.RemoteWindows;

    using TreeSharp;

    [XmlElement("TurnInCollectables")]
    public class TurnInCollectablesTag : ExProfileBehavior
    {
        private bool isDone;

        private bool turnedItemsIn;

        private BagSlot item;

        private uint index;

        private LocationData locationData;

        [DefaultValue(Locations.Idyllshire)]
        [Clio.XmlEngine.XmlAttribute("Location")]
        public Locations Location { get; set; }

        [Clio.XmlEngine.XmlAttribute("ForcePurchase")]
        public bool ForcePurchase { get; set; }

        [Clio.XmlEngine.XmlElement("Collectables")]
        public List<CollectableTurnIn> Collectables { get; set; }

        [Clio.XmlEngine.XmlElement("ShopPurchases")]
        public List<ShopPurchase> ShopPurchases { get; set; }

        public static uint GetClassIndex(ClassJobType classJobType)
        {
            return (uint)classJobType - 8;
        }

        public static void SelectClass(ClassJobType classJobType)
        {
            SelectClass(GetClassIndex(classJobType));
        }

        // Not needed but just because we can
        public static void SelectClass(uint index)
        {
            var window = RaptureAtkUnitManager.GetWindowByName("MasterPieceSupply");

            if (window == null)
            {
                RaptureAtkUnitManager.Update();
                window = RaptureAtkUnitManager.GetWindowByName("MasterPieceSupply");
            }

            if (window == null || !window.IsValid)
            {
                Logging.WriteDiagnostic(Colors.Red, "TurnInCollectables: MasterPieceSupply window unavailable.");
                return;
            }

            window.TrySendAction(2, 1, 2, 1, index);
        }

        protected override void OnStart()
        {
            locationData = Data.LocationMap[Location];
        }

        protected override void OnDone()
        {
            var window = RaptureAtkUnitManager.GetWindowByName("MasterPieceSupply");
            if (window != null)
            {
                window.TrySendAction(1, 3, uint.MaxValue);
            }

            window = RaptureAtkUnitManager.GetWindowByName("ShopExchangeCurrency");
            if (window != null)
            {
                window.TrySendAction(1, 3, uint.MaxValue);
            }
        }

        protected override void OnResetCachedDone()
        {
            isDone = false;
            turnedItemsIn = false;
            item = null;
            index = 0;
        }

        protected override Composite CreateBehavior()
        {
            return
                new ActionRunCoroutine(ctx => Main());
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

            return await ResolveItem()
                || HandleDeath()
                || await Behaviors.TeleportTo(locationData)
                || await MoveToNpc()
                || await InteractWithNpc()
                || await ResolveIndex()
                || await HandOver()
                || await HandleSkipPurchase()
                || await MoveToShopNpc()
                || await PurchaseItems();

        }

        private async Task<bool> HandleSkipPurchase()
        {
            if (ShopPurchases == null || ShopPurchases.Count == 0 || ShopPurchases.All(s => !ShouldPurchaseItem(s)))
            {
                Logging.Write("No items to purchase");
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
                    if (Scrips.BlueCrafter < info.Cost) return false;
                    break;
                case ShopType.RedCrafter:
                    if (Scrips.RedCrafter < info.Cost) return false;
                    break;
                case ShopType.BlueGatherer:
                    if (Scrips.BlueGatherer < info.Cost) return false;
                    break;
                case ShopType.RedGatherer:
                    if (Scrips.RedGatherer < info.Cost) return false;
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

            var itemsToPurchase = ShopPurchases.Where(ShouldPurchaseItem).ToArray();
            var npc = GameObjectManager.GetObjectByNPCId(locationData.ShopNpcId);
            var shopType = ShopType.BlueGatherer;
            var window = RaptureAtkUnitManager.GetWindowByName("ShopExchangeCurrency");
            foreach (var purchaseItem in itemsToPurchase)
            {
                var purchaseItemInfo = Data.ShopItemMap[purchaseItem.ShopItem];
                var purchaseItemData = purchaseItemInfo.ItemData;
                var ticks = 0;

                if (shopType != purchaseItemInfo.ShopType && window != null)
                {
                    ticks = 0;
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    while (window != null && window.IsValid && ticks++ < 10 && Behaviors.ShouldContinue)
                    {
                        var result = window.TrySendAction(1, 3, uint.MaxValue);
                        if (result == SendActionResult.InjectionError)
                        {
                            await Coroutine.Sleep(500);
                        }

                        await Coroutine.Wait(500, () => window == null || !window.IsValid);
                    }
                }

                shopType = purchaseItemInfo.ShopType;

                // target
                ticks = 0;
                while (Core.Target == null && (window == null || !window.IsValid) && ticks++ < 10 && Behaviors.ShouldContinue)
                {
                    npc.Target();
                    await Coroutine.Wait(1000, () => Core.Target != null);
                }

                // check for timeout
                if (ticks > 10)
                {
                    Logging.WriteDiagnostic(Colors.Red, "Timeout targeting npc.");
                    isDone = true;
                    return true;
                }

                // interact
                ticks = 0;
                while (!SelectIconString.IsOpen && (window == null || !window.IsValid) && ticks++ < 10 && Behaviors.ShouldContinue)
                {
                    npc.Interact();
                    await Coroutine.Wait(1000, () => SelectIconString.IsOpen);
                }

                // check for timeout
                if (ticks > 10)
                {
                    Logging.WriteDiagnostic(Colors.Red, "Timeout interacting with npc.");
                    isDone = true;
                    return true;
                }


                if (Location == Locations.MorDhona &&
                    (purchaseItemInfo.ShopType == ShopType.RedCrafter
                    || purchaseItemInfo.ShopType == ShopType.RedGatherer))
                {
                    Logging.Write(Colors.PaleVioletRed, "Unable to purchase item {0} in MorDhona, set location to Idyllshire.", purchaseItemData.EnglishName);
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

                    await
                        Coroutine.Wait(
                            5000,
                            () => (window = RaptureAtkUnitManager.GetWindowByName("ShopExchangeCurrency")) != null);
                }

                if (ticks > 5 || window == null || !window.IsValid)
                {
                    Logging.WriteDiagnostic(Colors.Red, "Timeout interacting with npc.");
                    if (SelectIconString.IsOpen)
                    {
                        SelectIconString.ClickLineEquals(SelectIconString.Lines().Last());
                    }

                    isDone = true;
                    return true;
                }

                await Coroutine.Sleep(600);

                while (purchaseItemData.ItemCount() < purchaseItem.MaxCount && Scrips.GetRemainingScripsByShopType(purchaseItemInfo.ShopType) >= purchaseItemInfo.Cost && Behaviors.ShouldContinue)
                {
                    ticks = 0;
                    while (!SelectYesno.IsOpen && ticks++ < 50 && Behaviors.ShouldContinue)
                    {
                        await Coroutine.Yield();
                        window.TrySendAction(2, 0, 0, 1, purchaseItemInfo.Index);
                        await Coroutine.Wait(200, () => SelectYesno.IsOpen);
                    }

                    if (ticks > 50 || !SelectYesno.IsOpen)
                    {
                        Logging.WriteDiagnostic(Colors.Red, "Timeout during purchase of {0}", purchaseItemData.EnglishName);
                        window.TrySendAction(1, 3, uint.MaxValue);
                        isDone = true;
                        return true;
                    }

                    var scripsLeft = Scrips.GetRemainingScripsByShopType(purchaseItemInfo.ShopType);
                    ticks = 0;
                    while (SelectYesno.IsOpen && ticks++ < 10 && Behaviors.ShouldContinue)
                    {
                        await Coroutine.Yield();
                        SelectYesno.ClickYes();
                        await Coroutine.Wait(1000, () => !SelectYesno.IsOpen);
                    }

                    if (ticks > 10 || SelectYesno.IsOpen)
                    {
                        Logging.WriteDiagnostic(Colors.Red, "Timeout during purchase of {0}", purchaseItemData.EnglishName);
                        SelectYesno.ClickNo();
                        await Coroutine.Yield();
                        window.TrySendAction(1, 3, uint.MaxValue);
                        isDone = true;
                        return true;
                    }

                    Logging.Write(
                        Colors.SpringGreen,
                        "Purchased item {0} for {1} {2} scrips at {3} ET",
                        purchaseItemData.EnglishName,
                        purchaseItemInfo.Cost,
                        purchaseItemInfo.ShopType,
                        WorldManager.EorzaTime);

                    // wait until scrips changed
                    await
                        Coroutine.Wait(
                            5000,
                            () => Scrips.GetRemainingScripsByShopType(purchaseItemInfo.ShopType) != scripsLeft);
                }

                await Coroutine.Sleep(1000);
            }

            Logging.Write(Colors.SpringGreen, "Purchases complete.");
            SelectYesno.ClickNo();
            if (SelectIconString.IsOpen)
            {
                SelectIconString.ClickLineEquals(SelectIconString.Lines().Last());    
            }
            
            window.TrySendAction(1, 3, uint.MaxValue);
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
            var ticks = 0;
            var window = RaptureAtkUnitManager.GetWindowByName("MasterPieceSupply");
            while (window == null && ticks++ < 60 && Behaviors.ShouldContinue)
            {
                RaptureAtkUnitManager.Update();
                window = RaptureAtkUnitManager.GetWindowByName("MasterPieceSupply");
                await Coroutine.Yield();
            }

            if (ticks > 60)
            {
                return false;
            }

            if (item == null || item.Item == null)
            {
                SelectYesno.ClickNo();
                if (window == null)
                {
                    return false;
                }

                ticks = 0;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                while (window != null && window.IsValid && ticks++ < 10 && Behaviors.ShouldContinue)
                {
                    var result = window.TrySendAction(1, 3, uint.MaxValue);
                    if (result == SendActionResult.InjectionError)
                    {
                        await Coroutine.Sleep(500);
                    }

                    await Coroutine.Wait(500, () => window == null || !window.IsValid);
                }
                
                return false;
            }

            if (SelectYesno.IsOpen)
            {
                Logging.Write(Colors.Red, "Full on scrips!");
                Blacklist.Add((uint)item.Pointer.ToInt32(), BlacklistFlags.Loot, TimeSpan.FromMinutes(3), "Don't turn in this item for 3 minutes, we are full on these scrips");
                item = null;
                index = 0;
                SelectYesno.ClickNo();
                window.TrySendAction(1, 3, uint.MaxValue);
                return true;
            }

            var requestAttempts = 0;
            while (!Request.IsOpen && requestAttempts++ < 20 && Behaviors.ShouldContinue)
            {
                await Coroutine.Yield();
                var result = window.TrySendAction(2, 0, 0, 1, index);
                if (result == SendActionResult.InjectionError)
                {
                    await Coroutine.Sleep(500);
                }

                await Coroutine.Wait(500, () => Request.IsOpen);
            }

            if (!Request.IsOpen)
            {
                Logging.Write(Colors.Red, "An error has occured while turning in the item");
                Blacklist.Add((uint)item.Pointer.ToInt32(), BlacklistFlags.Loot, TimeSpan.FromMinutes(3), "Don't turn in this item for 3 minutes, most likely it isn't a turn in option today.");
                item = null;
                index = 0;
                SelectYesno.ClickNo();
                window.TrySendAction(1, 3, uint.MaxValue);
                return true;
            }

            if (item == null || item.Item == null)
            {
                Logging.Write(Colors.Red, "The item has become null between the time we resolved it and tried to turn it in...");
                item = null;
                index = 0;
                await Coroutine.Yield();
                return true;
            }

            var attempts = 0;
            var itemName = item.Item.EnglishName;
            while (Request.IsOpen && attempts++ < 5 && Behaviors.ShouldContinue && item.Item != null)
            {
                item.Handover();
                await Coroutine.Wait(1000, () => Request.HandOverButtonClickable);
                Request.HandOver();
                await Coroutine.Wait(1000, () => !Request.IsOpen);
            }

            if (attempts < 6)
            {
                Logging.Write(
                    Colors.SpringGreen,
                    "Turned in {0} at {1} ET",
                    itemName,
                    WorldManager.EorzaTime);

                turnedItemsIn = true;
                item = null;
                index = 0;
                await Coroutine.Yield();
                return true;
            }

            Logging.Write(Colors.Red, "Too many attempts");
            Blacklist.Add((uint)item.Pointer.ToInt32(), BlacklistFlags.Loot, TimeSpan.FromMinutes(3), "Don't turn in this item for 3 minutes, something is wrong.");
            Request.Cancel();
            SelectYesno.ClickNo();
            window.TrySendAction(1, 3, uint.MaxValue);
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

            await
                Behaviors.MoveTo(
                    locationData.NpcLocation,
                    radius: 4.0f,
                    name: Location + " NpcId: " + locationData.NpcId);

            Navigator.Stop();

            return false;
        }

        private async Task<bool> InteractWithNpc()
        {
            if(item == null || item.Item == null)
            {
                return false;
            }

            if (Me.Location.Distance(locationData.NpcLocation) > 4)
            {
                // too far away, should go back to MoveToNpc
                return true;
            }

            if (GameObjectManager.Target != null && RaptureAtkUnitManager.GetWindowByName("MasterPieceSupply") != null)
            {
                // already met conditions
                return false;
            }

            var npc = GameObjectManager.GetObjectByNPCId(locationData.NpcId);
            npc.Target();
            npc.Interact();

            return false;
        }


        private async Task<bool> ResolveIndex()
        {
            if (item == null || item.Item == null)
            {
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
                classIndex = GetClassIndex((ClassJobType)item.Item.RepairClass);
            }
            else
            {
                switch (item.Item.EquipmentCatagory)
                {
                    case ItemUiCategory.Seafood:
                        classIndex = GetClassIndex(ClassJobType.Fisher);
                        break;
                    case ItemUiCategory.Stone:
                    case ItemUiCategory.Metal:
                    case ItemUiCategory.Bone:
                        classIndex = GetClassIndex(ClassJobType.Miner);
                        break;
                    case ItemUiCategory.Reagent:
                    case ItemUiCategory.Ingredient:
                    case ItemUiCategory.Lumber:
                        classIndex = GetClassIndex(ClassJobType.Botanist);
                        break;
                }

                if (classIndex == uint.MaxValue)
                {
                    Logging.Write(
                        Colors.Red,
                        "TurnInCollectables: Error, could not resolve class type for item: " + item.Item.EnglishName);
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
                    itemLevel = (byte)((itemLevel - 121) / 3);
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
                InventoryManager.FilledSlots.Where(
                    i => !Blacklist.Contains((uint)i.Pointer.ToInt32(), BlacklistFlags.Loot)).ToArray();

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
                            && string.Equals(
                                collectable.Name,
                                i.EnglishName,
                                StringComparison.InvariantCultureIgnoreCase));
                }
            }

            if ((item == null || item.Item == null) && ((!turnedItemsIn && !ForcePurchase) || await HandleSkipPurchase()))
            {
                isDone = true;
                return true;
            }

            // if we do resolve the item, return false so we just move on.
            return false;
        }
    }
}
