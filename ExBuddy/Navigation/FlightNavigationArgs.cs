namespace ExBuddy.Navigation
{
	using ExBuddy.Interfaces;

	public class FlightNavigationArgs : IFlightNavigationArgs
	{
		public FlightNavigationArgs()
		{
			Radius = 2.7f;
			InverseParabolicMagnitude = 6;
			Smoothing = 0.2f;
			ForcedAltitude = 8.0f;
		}

		public override string ToString()
		{
			return string.Concat(
				"R->",
				Radius,
				"IPM->",
				InverseParabolicMagnitude,
				"S->",
				Smoothing,
				"Alt->",
				ForcedAltitude);
		}

		#region IFlightNavigationArgs Members

		public float ForcedAltitude { get; set; }

		public int InverseParabolicMagnitude { get; set; }

		public float Radius { get; set; }

		public float Smoothing { get; set; }

		#endregion
	}
}