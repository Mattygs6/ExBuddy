using ExBuddy.Offsets;

namespace ExBuddy.Windows
{
    using ExBuddy.Enumerations;

    using ff14bot;
    using ff14bot.Managers;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    // 大国联防军列表
    public sealed class GrandCompanySupplyList : Window<GrandCompanySupplyList>
	{
		public GrandCompanySupplyList()
			: base("GrandCompanySupplyList") { }

        public SendActionResult TurnIn(uint index)
        {
            return TrySendAction(2, 0, 0, 1, index);
        }

        public SendActionResult HandOver(uint index)
        {
            return TrySendAction(2,1,index,0,0);
        }
        
    }
}
