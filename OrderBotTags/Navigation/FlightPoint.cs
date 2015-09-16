namespace ExBuddy.OrderBotTags.Navigation
{
    using System;

    using Clio.Utilities;

    public struct FlightPoint
    {
        public Vector3 Location;

        public bool IsDeviation;

        public bool FuzzyEquals(FlightPoint other)
        {
            if (other == Vector3.Zero)
            {
                return false;
            }

            return other.Location.Distance3D(this.Location) < 0.8f
                   || (Math.Abs(other.Location.X - this.Location.X) < float.Epsilon && Math.Abs(other.Location.Z - this.Location.Z) < float.Epsilon);
        }

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
