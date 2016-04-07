namespace ExBuddy.OrderBotTags.Objects
{
	using System.ComponentModel;
	using Clio.XmlEngine;

	[XmlElement("CollectableTurnIn")]
	public class CollectableTurnIn : CollectableBase
	{
		[DefaultValue(int.MaxValue)]
		[XmlAttribute("MaxValueForTurnIn")]
		public int MaxValueForTurnIn { get; set; }
	}
}