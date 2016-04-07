using System;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot.Managers;
using Clio.XmlEngine;
using ff14bot.RemoteWindows;
using System.Text.RegularExpressions;
using ff14bot.RemoteAgents;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;

namespace ExBuddy.OrderBotTags.Behaviors
{
    [XmlElement("Chocobo")]
    public class Chocobo : ExProfileBehavior
    {
        [DefaultValue(8165)]
        [XmlAttribute("FoodId")]
        public int FoodId { get; set; }
    
        [XmlAttribute("PlayerName")]
        public String PlayerName { get; set; }
        
        [XmlAttribute("ChocoboId")]
        public uint ChocoboId { get; set; }

        protected override async Task<bool> Main()
        {
            Log("目前仅支持克拉卡萝卜");
            FoodId = 8165;

            uint shack = 1073747699;
            GameObjectManager.GetObjectByObjectId(ChocoboId).Interact();
            await Coroutine.Sleep(2000);
            
            if (SelectString.IsOpen)
            {
                SelectString.ClickSlot(0);
                await Coroutine.Wait(5000, () => HousingChocoboList.IsOpen || HousingMyChocobo.IsOpen);
                //Give it sometime to populate data
                await Coroutine.Sleep(3000);
            }

            //Make sure we didn't just timeout above
            if (HousingChocoboList.IsOpen)
            {
                await HousingChocoboListWork();
            }
            else if (HousingMyChocobo.IsOpen)
            {
                await HousingMyChocoboWork();
            }
            else
            {
                 Log("陆行鸟棚没有打开");
            }

            if (HousingChocoboList.IsOpen)
            {
                HousingChocoboList.Close();
                await Coroutine.Sleep(2000);
            } else if(HousingMyChocobo.IsOpen)
            {
                HousingMyChocobo.Close();
                await Coroutine.Sleep(2000);
            }

            if (SelectString.IsOpen)
            {
                SelectString.ClickSlot(4);
                await Coroutine.Sleep(2000);
            }

            return isDone = true;
        }
        
       internal static Regex TimeRegex = new Regex(@"(?:.*?)(\d+).*", RegexOptions.Compiled);
        private async Task HousingMyChocoboWork()
        {
            var matches = TimeRegex.Match(HousingMyChocobo.Lines[0]);
            if (!matches.Success)
            {
                //We are ready to train now
                HousingMyChocobo.SelectLine(0);

                Log("Waiting for inventory menu to appear....");
                //Wait for the inventory window to open and be ready
                //Technically the inventory windows are always 'open' so we check if their callbackhandler has been set
                if (!await Coroutine.Wait(5000, () => AgentInventoryContext.Instance.CallbackHandlerSet))
                {
                    Log("Inventorymenu failed to appear, aborting current iteration and starting over.");
                    return;
                }

                Log("Feeding Chocobo {0}", FoodId);
                AgentHousingBuddyList.Instance.Feed((uint)FoodId);

                Log("Waiting for cutscene to start....");
                if (await Coroutine.Wait(5000, () => QuestLogManager.InCutscene))
                {
                    Log("Waiting for cutscene to end....");
                    await Coroutine.Wait(Timeout.Infinite, () => !QuestLogManager.InCutscene);
                }

                Log("Waiting for menu to reappear....");
                await Coroutine.Wait(Timeout.Infinite, () => HousingMyChocobo.IsOpen);
                await Coroutine.Sleep(1000);
            }
            else
            {
                var timeToSleep = TimeSpan.FromMinutes(Int32.Parse(matches.Groups[1].Value) + 1);
                HousingMyChocobo.Close();
                Log(@"Sleeping for {0},until our chocobo is ready.", timeToSleep);
                await Coroutine.Sleep(timeToSleep);
            }
        }

        private async Task HousingChocoboListWork()
        {
            //Look for our chocobo
            var items = HousingChocoboList.Items;
            var targetName = PlayerName;

            //512 possible chocobos, 14 items per page...
            for (uint stableSection = 0; stableSection < AgentHousingBuddyList.Instance.TotalPages; stableSection++)
            {

                if (stableSection != AgentHousingBuddyList.Instance.CurrentPage)
                {
                    Log("Switching to page {0}", stableSection);
                    HousingChocoboList.SelectSection(stableSection);
                    await Coroutine.Sleep(5000);
                    items = HousingChocoboList.Items;
                }
                
                for (uint i = 0; i < items.Length; i++)
                {
                    if (string.IsNullOrEmpty(items[i].PlayerName))
                        continue;

                    if (string.Equals(items[i].PlayerName, targetName, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(targetName))
                    {
                        if (items[i].ReadyAt < DateTime.Now)
                        {
                            //Personal chocobo is handled differently
                            if (i == 0)
                            {
                                Log("Selecting my Chocobo", i, stableSection);
                                HousingChocoboList.SelectMyChocobo();
                            }
                            else
                            {
                                Log("Selecting Chocobo {0} on page {1}", i, stableSection);
                                HousingChocoboList.SelectChocobo(i);
                            }

                            //检查陆行鸟是否满级了
                            if (await Coroutine.Wait(2500, () => SelectYesno.IsOpen))
                            {
                                Log("SelectYesNo window popped up, closing it and moving on");
                                SelectYesno.ClickNo();
                                await Coroutine.Sleep(1000);
                                continue;
                            }

                            Log("Waiting for inventory menu to appear....");
                            //Wait for the inventory window to open and be ready
                            //Technically the inventory windows are always 'open' so we check if their callbackhandler has been set
                            if (!await Coroutine.Wait(5000, () => AgentInventoryContext.Instance.CallbackHandlerSet))
                            {
                                Log("Inventorymenu failed to appear, aborting current iteration and starting over.");
                                continue;
                            }

                            Log("Feeding Chocobo {0}", FoodId);
                            AgentHousingBuddyList.Instance.Feed((uint)FoodId);

                            Log("Waiting for cutscene to start....");
                            if (await Coroutine.Wait(5000, () => QuestLogManager.InCutscene))
                            {
                                Log("Waiting for cutscene to end....");
                                await Coroutine.Wait(Timeout.Infinite, () => !QuestLogManager.InCutscene);
                            }

                            Log("Waiting for menu to reappear....");
                            await Coroutine.Wait(Timeout.Infinite, () => HousingChocoboList.IsOpen);
                            await Coroutine.Sleep(3000);
                        }
                    }
                }
            }
        }
    }
}
