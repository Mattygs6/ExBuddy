namespace ExBuddy.OrderBotTags.Fish
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using Clio.Utilities;
	using Clio.XmlEngine;
	using ff14bot;
	using ff14bot.Managers;

	[XmlElement("Bait")]
	public class Bait
	{
		internal Item BaitItem;

		private Func<bool> conditionFunc;

		[DefaultValue("True")]
		[XmlAttribute("Condition")]
		public string Condition { get; set; }

		[XmlAttribute("Id")]
		public uint Id { get; set; }

		[XmlAttribute("Name")]
		public string Name { get; set; }

		public static Bait FindMatch([NotNull] IList<Bait> baits)
		{
			var match = baits.FirstOrDefault(b => b.IsMatch()) ?? baits[0];

			return match;
		}

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
							i =>
								string.Equals(i.EnglishName, Name, StringComparison.InvariantCultureIgnoreCase)
								|| string.Equals(i.CurrentLocaleName, Name, StringComparison.InvariantCultureIgnoreCase));
				}
			}

			if (BaitItem == null || BaitItem.ItemCount() == 0)
			{
				return false;
			}

			if (Core.Player.ClassLevel < BaitItem.RequiredLevel)
			{
				return false;
			}

			return conditionFunc();
		}

		public override string ToString()
		{
			return this.DynamicToString();
		}
	}
}