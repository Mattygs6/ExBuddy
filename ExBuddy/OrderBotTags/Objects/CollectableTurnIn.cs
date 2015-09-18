namespace ExBuddy.OrderBotTags.Objects
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

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
}