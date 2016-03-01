namespace ExBuddy.OrderBotTags.Craft
{
    using System.Threading.Tasks;

    using Buddy.Coroutines;
    using Clio.XmlEngine;

    using ExBuddy.Attributes;
    using ExBuddy.OrderBotTags.Behaviors;

    using ff14bot;
    using ff14bot.Managers;
    using System.Threading;
    using ff14bot.RemoteWindows;
    [LoggerName("SimpleCraft")]
    [XmlElement("SimpleCraft")]
    public sealed class SimpleCraft : ExProfileBehavior
    {

        protected override async Task<bool> Main()
        {

            while (CraftingManager.IsCrafting && CraftingManager.ProgressRequired != CraftingManager.Progress)
            {
                Log("模范制作II");
                Actionmanager.DoAction(100069, Core.Me);

                await Coroutine.Wait(10000, () => CraftingManager.AnimationLocked);
                await Coroutine.Wait(Timeout.Infinite, () => !CraftingManager.AnimationLocked);
            }

            Log("制作结束");

            return isDone = true;
        }
    }
}