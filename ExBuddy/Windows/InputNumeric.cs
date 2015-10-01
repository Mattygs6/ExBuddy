namespace ExBuddy.Windows
{
	using ExBuddy.Enumerations;

	public sealed class InputNumeric : Window<InputNumeric>
	{
		public InputNumeric()
			: base("InputNumeric") {}

		public static SendActionResult AddOrRemoveCount(uint count)
		{
			return new InputNumeric().Count(count);
		}

		public SendActionResult Count(uint count)
		{
			return TrySendAction(1, 1, count);
		}
	}
}