#pragma warning disable 1998
namespace ExBuddy
{
    using System.Threading.Tasks;

    using Clio.Utilities;

    using ExBuddy.Interfaces;

    public class NoAetheryteUseAethernetReturnStrategy : IReturnStrategy
    {
        public ushort ZoneId { get; set; }

        public uint AetheryteId { get; set; }

        public Vector3 InitialLocation { get; set; }

        public uint AethernetIndex { get; set; }

        public async Task<bool> ReturnToZone()
        {
            return true;
        }

        public async Task<bool> ReturnToLocation()
        {
            return true;
        }
    }
}