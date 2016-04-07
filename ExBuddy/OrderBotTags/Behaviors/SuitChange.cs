using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot.Managers;
using Clio.XmlEngine;
using ff14bot;
using ff14bot.Behavior;
using ExBuddy.Attributes;
using System.ComponentModel;

namespace ExBuddy.OrderBotTags.Behaviors
{
    [LoggerName("Suit")]
    [XmlElement("Suit")]
    public class SuitChange : ExProfileBehavior
    {

        [XmlAttribute("SuitId")]
        public int SuitId { get; set; }

        [DefaultValue("")]
        [XmlAttribute("SuitName")]
        public string SuitName { get; set; }
    
        protected override async Task<bool> Main()
        {
            Log("准备更换套装{0},{1}",SuitId, SuitName);
            ChatManager.SendChat("/gs change "　+ SuitId);
            await Coroutine.Sleep(2500);

            if (Core.Player.CurrentJob == ff14bot.Enums.ClassJobType.Botanist && !Core.Player.HasAura(221))
            {
                 if(Core.Player.ClassLevel > 46)
                {
                    Actionmanager.DoAction(221, Core.Player);
                    await Coroutine.Sleep(2000);
                }
            } else if(Core.Player.CurrentJob == ff14bot.Enums.ClassJobType.Miner && !Core.Player.HasAura(222))
            {
                if (Core.Player.ClassLevel > 46)
                {
                    Actionmanager.DoAction(238, Core.Player);
                    await Coroutine.Sleep(2000);
                }
            }

            return isDone = true;
        }
        
    }
}
