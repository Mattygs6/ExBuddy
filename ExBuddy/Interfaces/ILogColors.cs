namespace ExBuddy.Interfaces
{
	using System.Windows.Media;

	public interface ILogColors
	{
		Color Error { get; }

		Color Info { get; }

		Color Warn { get; }
	}
}