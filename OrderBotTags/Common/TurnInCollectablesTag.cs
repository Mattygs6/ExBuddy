namespace ExBuddy.OrderBotTags.Common
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

    public struct LocationData
    {
        public uint AetheryteId { get; set; }

        public ushort ZoneId { get; set; }

        public uint NpcId { get; set; }

        public Vector3 NpcLocation { get; set; }
    }

    [XmlElement("TurnInCollectables")]
    public class TurnInCollectablesTag : ProfileBehavior
    {
        private static readonly Dictionary<Location, LocationData> LocationMap = new Dictionary<Location, LocationData>
            {
                { 
                    Location.MorDhona,
                    new LocationData
                    {
                        AetheryteId = 24,
                        ZoneId = 156,
                        NpcId = 1013396,
                        NpcLocation = new Vector3("50.33948, 31.13618, -737.4532")
                    }
                },
                { 
                    Location.Idyllshire,
                    new LocationData
                    {
                        AetheryteId = 75,
                        ZoneId = 478,
                        NpcId = 1012300,
                        NpcLocation = new Vector3("-15.64056, 211, 0.1677856")
                    }
                }
            };

        private bool isDone;

        private BagSlot item;

        private uint index;

        [DefaultValue(Location.Idyllshire)]
        [XmlAttribute("Location")]
        public Location Location { get; set; }

        [XmlElement("Collectables")]
        public List<Collectable> Collectables { get; set; }

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

            return await this.ResolveItem() || this.HandleDeath() || await this.TeleportToLocation()
                   || await this.MoveToNpc() || await this.InteractWithNpc() || await this.ResolveIndex()
                   || await this.HandOver();
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
            var window = RaptureAtkUnitManager.GetWindowByName("MasterPieceSupply");
            while (window == null)
            {
                RaptureAtkUnitManager.Update();
                window = RaptureAtkUnitManager.GetWindowByName("MasterPieceSupply");
                await Coroutine.Yield();
            }

            var requestAttempts = 0;
            while (!Request.IsOpen && requestAttempts < 5)
            {
                window.SendAction(2, 0, 0, 1, index);
                await Coroutine.Wait(3000, () => Request.IsOpen);
                requestAttempts++;
            }

            if (!Request.IsOpen)
            {
                Logging.Write(Colors.Red, "An error has occured while turning in the item");
                isDone = true;
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
                item = null;
                index = 0;
                return true;
            }

            Logging.Write(Colors.Red, "Too many attempts");

            isDone = true;
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
            switch (item.RawItemId)
            {
                case 12774U:
                case 12828U:
                    index = 9;
                    return false;
                case 12900U:
                    index = 11;
                    return false;
                case 12538U:
                    index = 13;
                    return false;
            }

            // No perfect algorithm for this, but will attempt.  Going to have to read the data from the window.

            var classIndex = uint.MaxValue;
            if (item.Item.RepairClass > 0)
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

            if (Collectables == null)
            {
                item = InventoryManager.FilledSlots.FirstOrDefault(i => i.Collectability > 0);
            }
            else
            {
                foreach (var collectable in Collectables)
                {
                    item =
                        InventoryManager.FilledSlots.FirstOrDefault(
                            i =>
                            i.Collectability >= collectable.Value
                            && string.Equals(
                                collectable.Name,
                                i.EnglishName,
                                StringComparison.InvariantCultureIgnoreCase));
                }
            }

            if (item == null)
            {
                isDone = true;
                return true;
            }

            // if we do resolve the item, return false so we just move on.
            return false;
        }
    }
}
