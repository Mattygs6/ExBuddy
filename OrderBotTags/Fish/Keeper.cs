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

        [Clio.XmlEngine.XmlAttribute("OnlyHq")]
        public bool OnlyHq { get; set; }
    }
}