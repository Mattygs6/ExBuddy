namespace ExBuddy.OrderBotTags.Behaviors.Objects
{
	using System;
	using System.ComponentModel;
	using System.Xml.Serialization;

	[XmlRoot(IsNullable = true, Namespace = "")]
	[Clio.XmlEngine.XmlElement("ShopPurchase")]
	[XmlType(AnonymousType = true)]
	[Serializable]
	public class ShopPurchase
	{
		[DefaultValue(198)]
		[Clio.XmlEngine.XmlAttribute("MaxCount")]
		public int MaxCount { get; set; }

		[DefaultValue(ShopItem.HiCordial)]
		[Clio.XmlEngine.XmlAttribute("ShopItem")]
		public ShopItem ShopItem { get; set; }

		public override string ToString()
		{
			return this.DynamicToString();
		}
	}
}