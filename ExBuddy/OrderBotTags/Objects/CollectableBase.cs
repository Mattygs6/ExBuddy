namespace ExBuddy.OrderBotTags.Objects
{
	using Clio.XmlEngine;
	using ExBuddy.Interfaces;

	public abstract class CollectableBase : INamedItem
	{
		[XmlAttribute("Value")]
		public int Value { get; set; }

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