namespace ExBuddy.Interfaces
{
	public interface INpc : ITeleportLocation, IInteractWithNpc
	{
		string Name { get; set; }
	}
}