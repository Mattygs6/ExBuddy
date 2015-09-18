namespace ExBuddy.OrderBotTags.Fish
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    using ExBuddy.Enums;

    [XmlRoot(IsNullable = true, Namespace = "")]
    [Clio.XmlEngine.XmlElement("Keeper")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class Keeper
    {
        [Clio.XmlEngine.XmlAttribute("Name")]
        public string Name { get; set; }

        [DefaultValue(KeeperAction.KeepAll)]
        [Clio.XmlEngine.XmlAttribute("Action")]
        public KeeperAction Action { get; set; }
    }
}