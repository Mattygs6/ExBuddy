

namespace ExBuddy.OrderBotTags.Common
{
    using System.Threading;
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using Clio.Utilities;

    using ff14bot;
    using ff14bot.Behavior;
    using ff14bot.Managers;
    using ff14bot.Navigation;
    using ff14bot.RemoteWindows;
    using ff14bot.Settings;

    public interface IReturnStrategy
    {
        ushort ZoneId { get; set; }

        uint AetheryteId { get; set; }

        Vector3 InitialLocation { get; set; }

        Task<bool> Execute();
    }

    public class NoOpReturnStrategy : IReturnStrategy
    {
        public ushort ZoneId { get; set; }

        public uint AetheryteId { get; set; }

        public Vector3 InitialLocation { get; set; }

        public async Task<bool> Execute()
        {
            return true;
        }
    }

    public class NoAetheryteUseAethernetReturnStrategy : IReturnStrategy
    {
        public ushort ZoneId { get; set; }

        public uint AetheryteId { get; set; }

        public Vector3 InitialLocation { get; set; }

        public uint AethernetIndex { get; set; }

        public async Task<bool> Execute()
        {

            return true;
        }
    }

    public class NoAetheryteRunToZoneReturnStrategy : IReturnStrategy
    {
        public ushort ZoneId { get; set; }

        public uint AetheryteId { get; set; }

        public Vector3 InitialLocation { get; set; }

        public Vector3 ZoneLocation { get; set; }

        public async Task<bool> Execute()
        {

            return true;
        }
    }

    public class NoAetheryteUseTransportReturnStrategy : IReturnStrategy
    {
        public NoAetheryteUseTransportReturnStrategy()
        {
            this.DialogOption = -1;
            this.InteractDistance = 4.0f;
        }

        public ushort ZoneId { get; set; }

        public uint AetheryteId { get; set; }

        public Vector3 InitialLocation { get; set; }

        public int DialogOption { get; set; }

        public float InteractDistance { get; set; }

        public uint NpcId { get; set; }

        public Vector3 XYZ { get; set; }

        public async Task<bool> Execute()
        {
            if (WorldManager.ZoneId != this.ZoneId)
            {
            ReturnTeleport:
                WorldManager.TeleportById(this.AetheryteId);
                if (await Coroutine.Wait(5000, () => Core.Player.IsCasting))
                {
                    await Coroutine.Wait(5000, () => !Core.Player.IsCasting);
                    await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading);
                    await Coroutine.Wait(Timeout.Infinite, () => !CommonBehaviors.IsLoading);
                }
                else
                {
                    goto ReturnTeleport;
                }
            }

            await Behaviors.MoveTo(this.XYZ, true, radius: this.InteractDistance);
            GameObjectManager.GetObjectByNPCId(this.NpcId).Target();
            Core.Player.CurrentTarget.Interact();

            // Temporarily assume selectyesno until we see if we need it for anything but hinterlands
            await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
            SelectYesno.ClickYes();

            await Coroutine.Wait(3000, () => CommonBehaviors.IsLoading);
            await Coroutine.Wait(Timeout.Infinite, () => !CommonBehaviors.IsLoading);

            if (Core.Player.Distance(this.InitialLocation) >= CharacterSettings.Instance.MountDistance)
            {
                Navigator.Stop();
                Actionmanager.Mount();
                await Coroutine.Sleep(1500);
            }

            if (BotManager.Current.EnglishName != "Fate Bot")
            {
                await Behaviors.MoveTo(this.InitialLocation);
            }
            else
            {
                await Coroutine.Sleep(1000);
            }

            return true;
        }
    }

    public class DefaultReturnStrategy : IReturnStrategy
    {
        public ushort ZoneId { get; set; }

        public uint AetheryteId { get; set; }

        public Vector3 InitialLocation { get; set; }

        public async Task<bool> Execute()
        {
            if (WorldManager.ZoneId != this.ZoneId)
            {
            ReturnTeleport:
                WorldManager.TeleportById(this.AetheryteId);
                if (await Coroutine.Wait(5000, () => Core.Player.IsCasting))
                {
                    await Coroutine.Wait(5000, () => !Core.Player.IsCasting);
                    await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading);
                    await Coroutine.Wait(Timeout.Infinite, () => !CommonBehaviors.IsLoading);
                }
                else
                {
                    goto ReturnTeleport;
                }
            }

            if (Core.Player.Distance(this.InitialLocation) >= CharacterSettings.Instance.MountDistance)
            {
                Actionmanager.Mount();
                await Coroutine.Sleep(2000);
            }

            if (BotManager.Current.EnglishName != "Fate Bot")
            {
                await Behaviors.MoveTo(this.InitialLocation);
            }
            else
            {
                await Coroutine.Sleep(1000);
            }

            return true;
        }
    }
    
}
