namespace ExBuddy.Interfaces
{
    public interface IFlightNavigationArgs
    {
        int InverseParabolicMagnitude { get; set; }

        float Smoothing { get; set; }

        float ForcedAltitude { get; set; }

        float Radius { get; set; }

        bool LogWaypoints { get; set; }
    }
}