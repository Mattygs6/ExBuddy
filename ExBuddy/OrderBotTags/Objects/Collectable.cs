namespace ExBuddy.OrderBotTags.Objects
{
    using System;
    using System.Xml.Serialization;

    [XmlRoot(IsNullable = true, Namespace = "")]
    [Clio.XmlEngine.XmlElement("Collectable")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class Collectable : CollectableBase
    {
        [Clio.XmlEngine.XmlAttribute("PlusPlus")]
        public int PlusPlus { get; set; }        
    }
}