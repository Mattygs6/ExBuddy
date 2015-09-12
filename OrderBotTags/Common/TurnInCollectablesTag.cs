namespace ExBuddy.OrderBotTags.Common
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using System.Xml.Serialization;

    using Buddy.Coroutines;

    using Clio.Utilities;
    using Clio.XmlEngine;

    using ff14bot;
    using ff14bot.Behavior;
    using ff14bot.Enums;
    using ff14bot.Helpers;
    using ff14bot.Managers;
    using ff14bot.Navigation;
    using ff14bot.NeoProfiles;
    using ff14bot.RemoteWindows;

    using TreeSharp;

    public enum Location
    {
        MorDhona,
        Idyllshire
    }

    public enum ShopItem
    {
        CrpDelineation = -8,
        BsmDelineation = -7,
        ArmDelineation = -6,
        GsmDelineation = -5,
        LtwDelineation = -4,
        WvrDelineation = -3,
        AlcDelineation = -2,
        CulDelineation = -1,
        RedCrafterToken = 0,
        RedGatherToken = 1,
        HiCordial = 6,
        BlueToken = 8,
        BruteLeech = 12,
        CraneFly = 13,
        KukuruPowder = 22,
        BouillonCube = 23,
        BeanSauce = 24,
        BeanPaste = 25
    }

    public enum ShopType
    {
        BlueCrafter,
        RedCrafter,
        BlueGatherer,
        RedGatherer
    }

    public struct ShopItemInfo
    {
        public uint Index { get; set; }
        public ShopType ShopType { get; set; }
        public uint ItemId { get; set; }
        public ushort Cost { get; set; }
        public byte Yield { get; set; }

        public Item ItemData
        {
            get
            {
                return DataManager.ItemCache[ItemId];
            }
        }
    }

    public static class Scrips
    {
        public static readonly IntPtr BasePointer = IntPtr.Add(Core.Memory.Process.MainModule.BaseAddress, 0x010379AC);

        public static int BlueCrafter
        {
            get
            {
                return Core.Memory.Read<int>(BasePointer);
            }
        }

        public static int RedCrafter
        {
            get
            {
                return Core.Memory.Read<int>(BasePointer + 8);
            }
        }

        public static int BlueGatherer
        {
            get
            {
                return Core.Memory.Read<int>(BasePointer + 16);
            }
        }

        public static int RedGatherer
        {
            get
            {
                return Core.Memory.Read<int>(BasePointer + 24);
            }
        }

        public static int GetRemainingScripsByShopType(ShopType shopType)
        {
            switch (shopType)
            {
                case ShopType.BlueCrafter:
                    return BlueCrafter;
                case ShopType.RedCrafter:
                    return RedCrafter;
                case ShopType.BlueGatherer:
                    return BlueGatherer;
                case ShopType.RedGatherer:
                    return RedGatherer;
            }

            return 0;
        }
    }

    public struct LocationData
    {
        public uint AetheryteId { get; set; }

        public ushort ZoneId { get; set; }

        public uint NpcId { get; set; }

        public Vector3 NpcLocation { get; set; }

        public uint ShopNpcId { get; set; }

        public Vector3 ShopNpcLocation { get; set; }
    }

    [XmlRoot(IsNullable = true, Namespace = "")]
    [Clio.XmlEngine.XmlElement("ShopPurchase")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class ShopPurchase
    {
        [Clio.XmlEngine.XmlAttribute("ShopItem")]
        public ShopItem ShopItem { get; set; }

        [DefaultValue(198)]
        [Clio.XmlEngine.XmlAttribute("MaxCount")]
        public int MaxCount { get; set; }
    }

    [Clio.XmlEngine.XmlElement("TurnInCollectables")]
    public class TurnInCollectablesTag : ProfileBehavior
    {
        private static readonly Dictionary<ShopItem, ShopItemInfo> ShopItemMap = new Dictionary<ShopItem, ShopItemInfo>
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

        private static readonly Dictionary<Location, LocationData> LocationMap = new Dictionary<Location, LocationData>
            {
                { 
                    Location.MorDhona,
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
                    Location.Idyllshire,
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

        private bool isDone;

        private bool turnedItemsIn;

        private BagSlot item;

        private uint index;

        [DefaultValue(Location.Idyllshire)]
        [Clio.XmlEngine.XmlAttribute("Location")]
        public Location Location { get; set; }

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

            window.SendAction(2, 1, 2, 1, index);
        }

        protected override void OnDone()
        {
            var window = RaptureAtkUnitManager.GetWindowByName("MasterPieceSupply");
            if (window != null)
            {
                window.SendAction(1, 3, uint.MaxValue);
            }

            window = RaptureAtkUnitManager.GetWindowByName("ShopExchangeCurrency");
            if (window != null)
            {
                window.SendAction(1, 3, uint.MaxValue);
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
                || await TeleportToLocation()
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
            var info = ShopItemMap[shopPurchase.ShopItem];

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
            var locationData = LocationMap[Location];
            if (GameObjectManager.LocalPlayer.Location.Distance(locationData.ShopNpcLocation) <= 4)
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
           
            var locationData = LocationMap[Location];
            if (GameObjectManager.LocalPlayer.Location.Distance(locationData.ShopNpcLocation) > 4)
            {
                // too far away, should go back to MoveToNpc
                return true;
            }

            var itemsToPurchase = ShopPurchases.Where(ShouldPurchaseItem).ToArray();
            var npc = GameObjectManager.GetObjectByNPCId(locationData.ShopNpcId);
            AtkAddonControl window = RaptureAtkUnitManager.GetWindowByName("ShopExchangeCurrency");
            foreach (var purchaseItem in itemsToPurchase)
            {
                // target
                var ticks = 0;
                while (Core.Target == null && window == null && ticks < 10)
                {
                    npc.Target();
                    await Coroutine.Yield();
                    ticks++;
                }

                // check for timeout
                if (ticks >= 10)
                {
                    Logging.WriteDiagnostic(Colors.Red, "Timeout targeting npc.");
                    isDone = true;
                    return true;
                }

                // interact
                ticks = 0;
                while (!SelectIconString.IsOpen && window == null && ticks < 10)
                {
                    npc.Interact();
                    await Coroutine.Wait(1000, () => SelectIconString.IsOpen);
                }

                // check for timeout
                if (ticks >= 10)
                {
                    Logging.WriteDiagnostic(Colors.Red, "Timeout interacting with npc.");
                    isDone = true;
                    return true;
                }

                var purchaseItemInfo = ShopItemMap[purchaseItem.ShopItem];
                var purchaseItemData = purchaseItemInfo.ItemData;

                if (purchaseItemInfo.ShopType == ShopType.RedCrafter
                    || purchaseItemInfo.ShopType == ShopType.RedGatherer)
                {
                    Logging.Write(Colors.PaleVioletRed, "Unable to purchase item {0} in MorDhona, set location to Idyllshire.", purchaseItemData.EnglishName);
                    continue;
                }

                ticks = 0;
                while (SelectIconString.IsOpen && ticks < 5)
                {
                    if (Location == Location.MorDhona)
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
                    ticks++;
                }

                if (ticks >= 5 || window == null)
                {
                    Logging.WriteDiagnostic(Colors.Red, "Timeout interacting with npc.");
                    SelectIconString.ClickLineEquals(SelectIconString.Lines().Last());
                    isDone = true;
                    return true;
                }

                // do this because if not we could get a trailblazer's scarf??
                await Coroutine.Sleep(1000);

                while (purchaseItemData.ItemCount() < purchaseItem.MaxCount && Scrips.GetRemainingScripsByShopType(purchaseItemInfo.ShopType) >= purchaseItemInfo.Cost)
                {
                    ticks = 0;
                    while (!SelectYesno.IsOpen && ticks < 3)
                    {
                        window.SendAction(2, 0, 0, 1, purchaseItemInfo.Index);
                        await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                    }

                    if (ticks >= 3 || !SelectYesno.IsOpen)
                    {
                        Logging.WriteDiagnostic(Colors.Red, "Timeout during purchase");
                        window.SendAction(1, 3, uint.MaxValue);
                        isDone = true;
                        return true;
                    }

                    var scripsLeft = Scrips.GetRemainingScripsByShopType(purchaseItemInfo.ShopType);
                    ticks = 0;
                    while (SelectYesno.IsOpen && ticks < 3)
                    {
                        SelectYesno.ClickYes();
                        await Coroutine.Wait(5000, () => !SelectYesno.IsOpen);
                    }

                    if (ticks >= 3 || SelectYesno.IsOpen)
                    {
                        Logging.WriteDiagnostic(Colors.Red, "Timeout during purchase");
                        SelectYesno.ClickNo();
                        await Coroutine.Yield();
                        window.SendAction(1, 3, uint.MaxValue);
                        isDone = true;
                        return true;
                    }

                    Logging.Write(
                        Colors.SpringGreen,
                        "Purchased item {0} for {1} {2} scrips!",
                        purchaseItemData.EnglishName,
                        purchaseItemInfo.Cost,
                        purchaseItemInfo.ShopType);

                    // wait until scrips changed
                    await
                        Coroutine.Wait(
                            5000,
                            () => Scrips.GetRemainingScripsByShopType(purchaseItemInfo.ShopType) != scripsLeft);
                }

                await Coroutine.Yield();
            }

            Logging.Write(Colors.SpringGreen, "Purchases complete.");

            window.SendAction(1, 3, uint.MaxValue);
            isDone = true;
            return true;
        }

        private bool HandleDeath()
        {
            if (Core.Player.IsDead && Poi.Current.Type != PoiType.Death)
            {
                Poi.Current = new Poi(Core.Player, PoiType.Death);
                return true;
            }

            return false;
        }

        private async Task<bool> HandOver()
        {
            var ticks = 0;
            var window = RaptureAtkUnitManager.GetWindowByName("MasterPieceSupply");
            while (window == null && ticks < 60)
            {
                RaptureAtkUnitManager.Update();
                window = RaptureAtkUnitManager.GetWindowByName("MasterPieceSupply");
                await Coroutine.Yield();
                ticks++;
            }

            if (ticks >= 60)
            {
                return false;
            }

            if (item == null)
            {
                SelectYesno.ClickNo();
                if (window == null)
                {
                    return false;
                }

                window.SendAction(1, 3, uint.MaxValue);
                return false;
            }

            if (SelectYesno.IsOpen)
            {
                Logging.Write(Colors.Red, "Full on scrips!");
                Blacklist.Add((uint)item.Pointer.ToInt32(), BlacklistFlags.Loot, TimeSpan.FromMinutes(3), "Don't turn in this item for 3 minutes, we are full on these scrips");
                SelectYesno.ClickNo();
                window.SendAction(1, 3, uint.MaxValue);
                return true;
            }

            var requestAttempts = 0;
            while (!Request.IsOpen && requestAttempts < 5)
            {
                window.SendAction(2, 0, 0, 1, index);
                await Coroutine.Wait(1500, () => Request.IsOpen);
                requestAttempts++;
            }

            if (!Request.IsOpen)
            {
                Logging.Write(Colors.Red, "An error has occured while turning in the item");
                return true;
            }

            var attempts = 0;
            while (Request.IsOpen && attempts < 5)
            {
                item.Handover();
                await Coroutine.Sleep(1000);
                Request.HandOver();
                await Coroutine.Wait(5000, () => !Request.IsOpen);
                attempts++;
            }

            if (attempts < 5)
            {
                turnedItemsIn = true;
                item = null;
                index = 0;
                return true;
            }

            Logging.Write(Colors.Red, "Too many attempts");

            SelectYesno.ClickNo();
            window.SendAction(1, 3, uint.MaxValue);
            return true;
        }

        private async Task<bool> TeleportToLocation()
        {
            var locationData = LocationMap[Location];
            if (WorldManager.ZoneId == locationData.ZoneId)
            {
                // continue we are in the zone.
                return false;
            }

            var casted = false;
            while (WorldManager.ZoneId != locationData.ZoneId && Core.Player.IsAlive
                   && !Core.Player.InCombat)
            {
                if (!Core.Player.IsCasting && casted)
                {
                    break;
                }

                if (!Core.Player.IsCasting && !CommonBehaviors.IsLoading)
                {
                    WorldManager.TeleportById(locationData.AetheryteId);
                    await Coroutine.Sleep(500);
                }

                casted = casted || Core.Player.IsCasting;
                await Coroutine.Yield();
            }

            await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading);
            await Coroutine.Wait(5000, () => !CommonBehaviors.IsLoading);

            return false;
        }

        private async Task<bool> MoveToNpc()
        {
            if (item == null)
            {
                return false;
            }

            var locationData = LocationMap[Location];
            if (GameObjectManager.LocalPlayer.Location.Distance(locationData.NpcLocation) <= 4)
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
            if(item == null)
            {
                return false;
            }

            var locationData = LocationMap[Location];
            if (GameObjectManager.LocalPlayer.Location.Distance(locationData.NpcLocation) > 4)
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
            if (item == null)
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

            var indexOffset = 0;

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
                            i.Collectability >= collectable.Value
                            && string.Equals(
                                collectable.Name,
                                i.EnglishName,
                                StringComparison.InvariantCultureIgnoreCase));
                }
            }

            if (item == null && ((!turnedItemsIn && !ForcePurchase) || await HandleSkipPurchase()))
            {
                isDone = true;
                return true;
            }

            // if we do resolve the item, return false so we just move on.
            return false;
        }
    }
}
