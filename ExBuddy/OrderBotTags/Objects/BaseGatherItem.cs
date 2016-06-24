namespace ExBuddy.OrderBotTags.Objects
{
    using Clio.Utilities;
    using Clio.XmlEngine;

    using ExBuddy.Interfaces;
    using ff14bot.NeoProfiles;
    using System;
    using System.ComponentModel;
	public abstract class BaseGatherItem : IConditionNamedItem
	{
        protected Func<bool> condition;

        #region INamedItem  Members

        [XmlAttribute("Id")]
        public uint Id { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("LocalName")]
        public string LocalName { get; set; }

        #endregion

        #region IConditionNamedItem Members

        [DefaultValue("True")]
        [XmlAttribute("Condition")]
        public string Condition { get; set; }

        [DefaultValue(0)]
        [XmlAttribute("ItemCount")]
        public int ItemCount { get; set; }

        [DefaultValue(0)]
        [XmlAttribute("HqItemCount")]
        public int HqItemCount { get; set; }

        [DefaultValue(0)]
        [XmlAttribute("NqItemCount")]
        public int NqItemCount { get; set; }

        #endregion

        public abstract bool ConditionResult { get; }

		public override string ToString()
		{
			return this.DynamicToString();
		}
	}
}