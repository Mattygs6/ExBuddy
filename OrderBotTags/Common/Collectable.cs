namespace ExBuddy.OrderBotTags.Common
{
    using System;
    using System.ComponentModel;
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

    [XmlRoot(IsNullable = true, Namespace = "")]
    [Clio.XmlEngine.XmlElement("CollectableTurnIn")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class CollectableTurnIn : CollectableBase
    {
        [DefaultValue(int.MaxValue)]
        [Clio.XmlEngine.XmlAttribute("MaxValueForTurnIn")]
        public int MaxValueForTurnIn { get; set; }
    }

    public abstract class CollectableBase
    {
        [Clio.XmlEngine.XmlAttribute("Name")]
        public string Name { get; set; }

        [Clio.XmlEngine.XmlAttribute("Value")]
        public int Value { get; set; }
    }
}