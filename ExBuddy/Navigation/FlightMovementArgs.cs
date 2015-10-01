namespace ExBuddy.Navigation
{
	using ExBuddy.Interfaces;

	public class FlightMovementArgs : IFlightMovementArgs
	{
		#region IFlightMovementArgs Members

		public bool ForceLanding { get; set; }

		public int MountId { get; set; }

		#endregion
	}
}