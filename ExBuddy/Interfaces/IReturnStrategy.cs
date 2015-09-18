namespace ExBuddy.Interfaces
{
    using System.Threading.Tasks;

    using Clio.Utilities;

    public interface IReturnStrategy
    {
        ushort ZoneId { get; set; }

        uint AetheryteId { get; set; }

        Vector3 InitialLocation { get; set; }

        Task<bool> ReturnToZone();

        Task<bool> ReturnToLocation();
    }
}