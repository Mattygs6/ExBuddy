namespace ExBuddy.Interfaces
{
	using System.Windows.Media;

	public interface ILogColors
	{
		Color Error { get; }

		Color Warn { get; }

		Color Info { get; }
	}
}