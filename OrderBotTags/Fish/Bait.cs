namespace ExBuddy.OrderBotTags
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Xml.Serialization;

    using Clio.Utilities;

    using ff14bot.Managers;

    [XmlRoot(IsNullable = true, Namespace = "")]
    [Clio.XmlEngine.XmlElement("Bait")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class Bait
    {
        private Func<bool> conditionFunc;

        internal Item BaitItem;
        
        [Clio.XmlEngine.XmlAttribute("Name")]
        public string Name { get; set; }

        [Clio.XmlEngine.XmlAttribute("Id")]
        public uint Id { get; set; }

        [DefaultValue("True")]
        [Clio.XmlEngine.XmlAttribute("Condition")]
        public string Condition { get; set; }

        public bool IsMatch()
        {
            if (conditionFunc == null)
            {
                conditionFunc = ScriptManager.GetCondition(Condition);
            }

            if (BaitItem == null)
            {
                if (Id > 0)
                {
                    BaitItem = DataManager.ItemCache[Id];
                }
                else if (!string.IsNullOrWhiteSpace(Name))
                {
                    BaitItem =
                        DataManager.ItemCache.Values.Find(
                            i => string.Equals(i.EnglishName, Name, StringComparison.InvariantCultureIgnoreCase)
                                || string.Equals(i.CurrentLocaleName, Name, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            if (BaitItem == null || BaitItem.ItemCount() == 0)
            {
                return false;
            }

            return conditionFunc();
        }

        public static Bait FindMatch([NotNull]IList<Bait> baits)
        {
            var match = baits.FirstOrDefault(b => b.IsMatch()) ?? baits[0];

            return match;
        }
    }
}