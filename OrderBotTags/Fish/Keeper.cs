namespace ExBuddy.OrderBotTags
{
    using System;
    using System.Xml.Serialization;

    [XmlRoot(IsNullable = true, Namespace = "")]
    [Clio.XmlEngine.XmlElement("Keeper")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class Keeper
    {
        [Clio.XmlEngine.XmlAttribute("Name")]
        public string Name { get; set; }

        [Clio.XmlEngine.XmlAttribute("HqOnly")]
        public bool HqOnly { get; set; }
    }
}