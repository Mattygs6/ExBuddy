namespace ExBuddy.Navigation
{
	using System;
	using System.Globalization;

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

		public string GetCacheKey()
		{
			var x = this.Location.X.ToString("F2", CultureInfo.InvariantCulture);
			var y = this.Location.Y.ToString("F2", CultureInfo.InvariantCulture);
			var z= this.Location.Z.ToString("F2", CultureInfo.InvariantCulture);

			return string.Concat(x, ',', y, ',', z);
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
			return Location + (IsDeviation ? " σ" : string.Empty);
		}
	}
}