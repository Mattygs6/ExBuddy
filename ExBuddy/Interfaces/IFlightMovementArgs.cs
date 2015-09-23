namespace ExBuddy.Interfaces
{
	public interface IFlightMovementArgs
	{
		int MountId { get; set; }

		bool ForceLanding { get; set; }
	}
}