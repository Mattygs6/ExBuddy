namespace ExBuddy.Interfaces
{
	using Clio.Utilities;

	public interface IInteractWithNpc
	{
		Vector3 Location { get; set; }

		uint NpcId { get; set; }
	}
}