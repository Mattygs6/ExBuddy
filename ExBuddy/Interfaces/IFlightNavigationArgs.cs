namespace ExBuddy.Interfaces
{
	public interface IFlightNavigationArgs
	{
		float ForcedAltitude { get; set; }

		int InverseParabolicMagnitude { get; set; }

		float Radius { get; set; }

		float Smoothing { get; set; }
	}
}