namespace ExBuddy.OrderBotTags.Objects
{
    public abstract class CollectableBase
    {
        [Clio.XmlEngine.XmlAttribute("Name")]
        public string Name { get; set; }

        [Clio.XmlEngine.XmlAttribute("Value")]
        public int Value { get; set; }
    }
}