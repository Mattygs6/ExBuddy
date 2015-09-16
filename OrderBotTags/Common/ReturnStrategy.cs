namespace ExBuddy.OrderBotTags.Common
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using Buddy.Coroutines;

    using Clio.Utilities;

    using ff14bot;
    using ff14bot.Behavior;
    using ff14bot.Helpers;
    using ff14bot.Managers;
    using ff14bot.RemoteWindows;

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
            Logging.Write(Colors.DarkKhaki, "ExBuddy: Could not find a return strategy for ZoneId: {0}", ZoneId);
            return true;
        }

        public override string ToString()
        {
            return
                "NoOp: Can't figure out what we are supposed to do, hopefully someone else can help us.";
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

        public Vector3 NpcLocation { get; set; }

        public async Task<bool> Execute()
        {
            await Behaviors.TeleportTo(this);

            await Behaviors.MoveTo(NpcLocation, true, radius: InteractDistance);
            GameObjectManager.GetObjectByNPCId(NpcId).Target();
            Core.Player.CurrentTarget.Interact();

            // Temporarily assume selectyesno until we see if we need it for anything but hinterlands
            await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
            SelectYesno.ClickYes();

            await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading);
            await Coroutine.Wait(Timeout.Infinite, () => !CommonBehaviors.IsLoading);

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

        public override string ToString()
        {
            return string.Format(
                "NoAetheryteUseTransport: Death Location: {0}, AetheryteId: {1}, NpcLocation: {2}",
                InitialLocation,
                AetheryteId,
                NpcLocation);
        }
    }

    public class DefaultReturnStrategy : IReturnStrategy
    {
        public ushort ZoneId { get; set; }

        public uint AetheryteId { get; set; }

        public Vector3 InitialLocation { get; set; }

        public async Task<bool> Execute()
        {
            await Behaviors.TeleportTo(this);

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

        public override string ToString()
        {
            return string.Format(
                "Default: Death Location: {0}, AetheryteId: {1}",
                InitialLocation,
                AetheryteId);
        }
    }
    
}
