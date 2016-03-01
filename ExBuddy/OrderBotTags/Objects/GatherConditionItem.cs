namespace ExBuddy.OrderBotTags.Objects
{
    using Clio.Utilities;
    using Clio.XmlEngine;

    using ExBuddy.Interfaces;
    using System.ComponentModel;
    [XmlElement("ConditionItem")]
	public class GatherConditionItem : GatherItem
    {
        [DefaultValue("true")]
        [XmlAttribute("Condition")]
        public string Condition { get; set; }

    }
}