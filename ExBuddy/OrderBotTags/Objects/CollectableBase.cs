namespace ExBuddy.OrderBotTags.Objects
{
    using Clio.XmlEngine;

    using ExBuddy.Interfaces;
    using System.ComponentModel;
    public abstract class CollectableBase : IConditionNamedItem
	{
		[XmlAttribute("Name")]
		public string Name { get; set; }

        [DefaultValue("True")]
        [XmlAttribute("Condition")]
        public string Condition { set; get; }

		[XmlAttribute("Value")]
		public int Value { get; set; }

		public override string ToString()
		{
			return this.DynamicToString();
		}
	}
}