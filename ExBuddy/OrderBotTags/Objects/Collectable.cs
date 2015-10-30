namespace ExBuddy.OrderBotTags.Objects
{
	using Clio.XmlEngine;

	[XmlElement("Collectable")]
	public class Collectable : CollectableBase
	{
		[XmlAttribute("PlusPlus")]
		public int PlusPlus { get; set; }
	}
}