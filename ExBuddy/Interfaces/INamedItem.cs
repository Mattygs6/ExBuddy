namespace ExBuddy.Interfaces
{
	public interface INamedItem
	{
		uint Id { get; set; }

		string Name { get; set; }

		string LocalName { get; set; }
	}
}