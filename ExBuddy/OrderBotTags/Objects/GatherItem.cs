namespace ExBuddy.OrderBotTags.Objects
{
	using Clio.XmlEngine;

	using ExBuddy.Interfaces;

	[XmlElement("GatherItem")]
	public class GatherItem : INamedItem
	{
		[XmlAttribute("Name")]
		public string Name { get; set; }

		public override string ToString()
		{
			return this.DynamicToString();
		}
	}
}
