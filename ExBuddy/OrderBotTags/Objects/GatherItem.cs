namespace ExBuddy.OrderBotTags.Objects
{
	using Clio.XmlEngine;
	using ExBuddy.Interfaces;

	[XmlElement("GatherItem")]
	public class GatherItem : INamedItem
	{
		#region INamedItem Members

		[XmlAttribute("Id")]
		public uint Id { get; set; }

		[XmlAttribute("Name")]
		public string Name { get; set; }

		[XmlAttribute("LocalName")]
		public string LocalName { get; set; }

		#endregion

		public override string ToString()
		{
			return this.DynamicToString();
		}
	}
}