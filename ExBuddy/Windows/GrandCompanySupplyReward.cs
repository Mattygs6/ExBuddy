using ExBuddy.Offsets;

namespace ExBuddy.Windows
{
    using ExBuddy.Enumerations;

    using ff14bot;
    using ff14bot.Managers;
    using System.Threading.Tasks;

    // 大国联防军列表
    public sealed class GrandCompanySupplyReward : Window<GrandCompanySupplyReward>
	{
		public GrandCompanySupplyReward()
			: base("GrandCompanySupplyReward") { }

        public SendActionResult Yes()
        {
            return TrySendAction(1, 3, 0);
        }

        public SendActionResult No()
        {
            return TrySendAction(1, 3, 1);
        }

    }
}
