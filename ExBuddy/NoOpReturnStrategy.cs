#pragma warning disable 1998
namespace ExBuddy
{
    using System.Threading.Tasks;
    using System.Windows.Media;

    using Clio.Utilities;

    using ExBuddy.Interfaces;

    using ff14bot.Helpers;

    public class NoOpReturnStrategy : IReturnStrategy
    {
        public ushort ZoneId { get; set; }

        public uint AetheryteId { get; set; }

        public Vector3 InitialLocation { get; set; }

        public async Task<bool> ReturnToZone()
        {
            // TODO Global logger
            ////Logging.Write(Colors.DarkKhaki, "ExBuddy: Could not find a return strategy for ZoneId: {0}", this.ZoneId);
            return true;
        }

        public async Task<bool> ReturnToLocation()
        {
            return true;
        }

        public override string ToString()
        {
            return
                "NoOp: Can't figure out what we are supposed to do, hopefully someone else can help us.";
        }
    }
}