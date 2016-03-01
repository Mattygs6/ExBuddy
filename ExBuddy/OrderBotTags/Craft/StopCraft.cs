namespace ExBuddy.OrderBotTags.Craft
{
    using Buddy.Coroutines;
    using Clio.XmlEngine;
    using ExBuddy.Attributes;
    using ExBuddy.OrderBotTags.Behaviors;
    using ff14bot.Managers;
    using ff14bot.RemoteWindows;
    using System.Threading.Tasks;

    [LoggerName("StopCraft")]
    [XmlElement("StopCraft")]
    public sealed class StopCraft : ExProfileBehavior
    {

        protected override async Task<bool> Main()
        {
            if(CraftingManager.IsCrafting)
            {
                
            }

            if(CraftingLog.IsOpen)
            {
                CraftingLog.Close();
                await Coroutine.Sleep(2500);
            }

            return isDone = true;
        }
    }
}