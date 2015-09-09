namespace ExBuddy.OrderBotTags.Navigation
{    
    using Clio.Utilities;

    public struct FlightPoint
    {
        public Vector3 Location;

        public bool IsDeviation;

        public override string ToString()
        {
            return this.Location + (IsDeviation ? " *D*" : string.Empty);
        }

        public static implicit operator Vector3(FlightPoint flightPoint)
        {
            return flightPoint.Location;
        }

        public static implicit operator FlightPoint(Vector3 vector)
        {
            return new FlightPoint { Location = vector };
        }
    }
}
