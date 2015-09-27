namespace ExBuddy.Windows
{
	using ExBuddy.Enumerations;

	public sealed class SelectYesnoCount : Window<SelectYesnoCount>
	{
		public SelectYesnoCount()
			: base("SelectYesnoCount") {}

		public SendActionResult Yes()
		{
			return TrySendAction(1, 3, 0);
		}
	}
}