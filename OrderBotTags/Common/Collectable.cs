namespace ExBuddy.OrderBotTags
{
    using System;
    using System.Xml.Serialization;

    [XmlRoot(IsNullable = true, Namespace = "")]
    [Clio.XmlEngine.XmlElement("Collectable")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class Collectable
    {
        [Clio.XmlEngine.XmlAttribute("Name")]
        public string Name { get; set; }

        [Clio.XmlEngine.XmlAttribute("Value")]
        public int Value { get; set; }
    }
}