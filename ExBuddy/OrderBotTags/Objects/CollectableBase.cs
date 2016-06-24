namespace ExBuddy.OrderBotTags.Objects
{
    using Clio.Utilities;
    using Clio.XmlEngine;

    public abstract class CollectableBase : BaseGatherItem
    {
		[XmlAttribute("Value")]
		public int Value { get; set; }

        public override bool ConditionResult
        {
            get
            {
                if (condition == null)
                {
                    condition = ScriptManager.GetCondition(Condition);
                }
                return condition();
            }
        }

        public override string ToString()
		{
			return this.DynamicToString();
		}
	}
}