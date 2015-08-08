namespace ExBuddy.OrderBotTags
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

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