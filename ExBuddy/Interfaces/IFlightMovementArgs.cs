namespace ExBuddy.Interfaces
{
	public interface IFlightMovementArgs
	{
		bool ForceLanding { get; set; }

		int MountId { get; set; }
	}
}