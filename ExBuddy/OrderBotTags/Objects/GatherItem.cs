namespace ExBuddy.OrderBotTags.Objects
{
    using Clio.XmlEngine;

    using ExBuddy.Interfaces;
    using System.ComponentModel;
    [XmlElement("GatherItem")]
	public class GatherItem : IConditionNamedItem
	{
		[XmlAttribute("Name")]
		public string Name { get; set; }

        [DefaultValue("True")]
        [XmlAttribute("Condition")]
        public string Condition { get; set; }

		public override string ToString()
		{
			return this.DynamicToString();
		}
	}
}
