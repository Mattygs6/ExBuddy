namespace ExBuddy.OrderBotTags.Behaviors
{
    using System.Threading.Tasks;
    using Clio.XmlEngine;
    using Buddy.Coroutines;

    using ff14bot.Managers;
    using ff14bot.RemoteWindows;

    [XmlElement("Retainer")]
    public class Retainer : ExProfileBehavior
    {
        protected override async Task<bool> Main()
        {
            GameObjectManager.GetObjectByNPCId(2000401).Interact();
            if (await Coroutine.Wait(5000, () => SelectString.IsOpen))
            {
                uint count = 0;
                int lineC = SelectString.LineCount;
                uint countLine = (uint)lineC;
                foreach (var retainer in SelectString.Lines())
                {
                    if (retainer.ToString().EndsWith("]") || retainer.ToString().EndsWith(")"))
                    {
                        Log("Checking Retainer n° " + (count + 1));
                        if (retainer.ToString().EndsWith("[Tâche terminée]") || retainer.ToString().EndsWith("(Venture complete)") || retainer.ToString().EndsWith("[探险归来]"))
                        {
                            Log("Venture Completed !");
                            SelectString.ClickSlot(count);
                            if (await Coroutine.Wait(5000, () => Talk.DialogOpen))
                            {
                                Talk.Next();
                                if (await Coroutine.Wait(5000, () => SelectString.IsOpen))
                                {
                                    SelectString.ClickSlot(5);
                                    if (await Coroutine.Wait(5000, () => RetainerTaskResult.IsOpen))
                                    {
                                        RetainerTaskResult.Reassign();
                                        if (await Coroutine.Wait(5000, () => RetainerTaskAsk.IsOpen))
                                        {
                                            RetainerTaskAsk.Confirm();
                                            if (await Coroutine.Wait(5000, () => Talk.DialogOpen))
                                            {
                                                Talk.Next();
                                                if (await Coroutine.Wait(54000, () => SelectString.IsOpen))
                                                {
                                                    SelectString.ClickSlot(9);
                                                    if (await Coroutine.Wait(5000, () => Talk.DialogOpen))
                                                    {
                                                        Talk.Next();
                                                        await Coroutine.Sleep(3000);
                                                        GameObjectManager.GetObjectByNPCId(2000401).Interact();
                                                        if (await Coroutine.Wait(5000, () => SelectString.IsOpen))
                                                        {
                                                            count++;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log("Venture not Completed !");
                            count++;
                        }
                    }
                    else
                    {
                        Log("No more Retainer to check");
                        SelectString.ClickSlot(countLine - 1);
                    }
                }
                return isDone = true;
            }
            return isDone = true;
        }
    }
}
