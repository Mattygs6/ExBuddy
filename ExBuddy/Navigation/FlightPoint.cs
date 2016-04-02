namespace ExBuddy.Navigation
{
	using System;
	using Clio.Utilities;

	public struct FlightPoint
	{
		public bool IsDeviation;

		public Vector3 Location;

		public bool FuzzyEquals(FlightPoint other)
		{
			if (other == Vector3.Zero)
			{
				return false;
			}

			return other.Location.Distance3D(Location) < 0.8f
			       || (Math.Abs(other.Location.X - Location.X) < float.Epsilon
			           && Math.Abs(other.Location.Z - Location.Z) < float.Epsilon);
		}

		public static implicit operator Vector3(FlightPoint flightPoint)
		{
			return flightPoint.Location;
		}

		public static implicit operator FlightPoint(Vector3 vector)
		{
			return new FlightPoint {Location = vector};
		}

		public override string ToString()
		{
			return Location + (IsDeviation ? " *D*" : string.Empty);
		}
	}
}