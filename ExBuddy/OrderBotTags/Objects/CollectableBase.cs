namespace ExBuddy.OrderBotTags.Objects
{
	using Clio.XmlEngine;

	public abstract class CollectableBase
	{
		[XmlAttribute("Name")]
		public string Name { get; set; }

		[XmlAttribute("Value")]
		public int Value { get; set; }

		public override string ToString()
		{
			return this.DynamicToString();
		}
	}
}