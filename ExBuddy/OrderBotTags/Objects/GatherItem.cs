namespace ExBuddy.OrderBotTags.Objects
{
    using Clio.Utilities;
    using Clio.XmlEngine;
    using ff14bot.NeoProfiles;
    [XmlElement("GatherItem")]
	public class GatherItem : BaseGatherItem
    {
        public override bool ConditionResult
        {
            get
            {
                if (condition == null)
                {
                    condition = ScriptManager.GetCondition(Condition);
                }
                return condition()
                    || (ItemCount > 0 && ConditionParser.ItemCount(Name) < ItemCount)
                    || (HqItemCount > 0 && ConditionParser.HqItemCount(Name) < HqItemCount)
                    || (NqItemCount > 0 && ConditionParser.NqItemCount(Name) < NqItemCount);
            }
        }

        public override string ToString()
		{
			return this.DynamicToString();
		}
	}
}