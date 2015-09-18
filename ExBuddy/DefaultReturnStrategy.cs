namespace ExBuddy
{
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using Clio.Utilities;

    using ExBuddy.Helpers;
    using ExBuddy.Interfaces;

    using ff14bot.Managers;

    public class DefaultReturnStrategy : IReturnStrategy
    {
        public ushort ZoneId { get; set; }

        public uint AetheryteId { get; set; }

        public Vector3 InitialLocation { get; set; }

        public async Task<bool> ReturnToZone()
        {
            await Behaviors.TeleportTo(this);

            return true;
        }

        public async Task<bool> ReturnToLocation()
        {
            if (BotManager.Current.EnglishName != "Fate Bot")
            {
                return await Behaviors.MoveTo(this.InitialLocation);
            }

            await Coroutine.Sleep(1000);
            return true;
        }

        public override string ToString()
        {
            return string.Format(
                "Default: Death Location: {0}, AetheryteId: {1}",
                this.InitialLocation,
                this.AetheryteId);
        }
    }
}
