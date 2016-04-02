namespace ExBuddy.OrderBotTags.Objects
{
	using Clio.XmlEngine;
	using ExBuddy.Interfaces;

	[XmlElement("GatherItem")]
	public class GatherItem : INamedItem
	{
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