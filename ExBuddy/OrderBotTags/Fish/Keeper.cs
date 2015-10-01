namespace ExBuddy.OrderBotTags.Fish
{
	using System;
	using System.ComponentModel;
	using System.Xml.Serialization;

	using ExBuddy.Enumerations;

	[XmlRoot(IsNullable = true, Namespace = "")]
	[Clio.XmlEngine.XmlElement("Keeper")]
	[XmlType(AnonymousType = true)]
	[Serializable]
	public class Keeper
	{
		[DefaultValue(KeeperAction.KeepAll)]
		[Clio.XmlEngine.XmlAttribute("Action")]
		public KeeperAction Action { get; set; }

		[Clio.XmlEngine.XmlAttribute("Name")]
		public string Name { get; set; }

		public override string ToString()
		{
			return string.Concat("{ Name: ", Name, ", Action: ", Action, " }");
		}
	}
}