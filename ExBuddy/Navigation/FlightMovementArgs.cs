namespace ExBuddy.Navigation
{
    using ExBuddy.Interfaces;

    public class FlightMovementArgs : IFlightMovementArgs
    {
        public int MountId { get; set; }

        public bool ForceLanding { get; set; }
    }
}