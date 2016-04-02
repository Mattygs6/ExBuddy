namespace ExBuddy.OrderBotTags.Objects
{
	using Clio.XmlEngine;
	using ExBuddy.Interfaces;

	public abstract class CollectableBase : INamedItem
	{
		[XmlAttribute("Value")]
		public int Value { get; set; }

		#region INamedItem Members

		[XmlAttribute("Name")]
		public string Name { get; set; }

		#endregion

		public override string ToString()
		{
			return this.DynamicToString();
		}
	}
}