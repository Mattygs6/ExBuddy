namespace ExBuddy.OrderBotTags.Craft.Impl
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
    using ff14bot.Enums;
    using ff14bot.Objects;
    using System.ComponentModel;
    [LoggerName("Level60ZeroStarCraft")]
    [XmlElement("Level60ZeroStarCraft")]
    public sealed class Level60ZeroStarCraft : ExProfileBehavior
    {
        [DefaultValue(0)]
        [XmlAttribute("CollectValue")]
        public int CollectValue { get; set; }

        protected override async Task<bool> Main()
        {
            await Coroutine.Sleep(1000);

            SpellData data = null;

            // 检查
            if (Core.Me.CurrentCP < 404)
            {
                LogError("最低制作力需要404，当前制作力{0}", Core.Me.CurrentCP);
                return false;
            }

            if (!await checkSkills(data))
            {
                return false;
            }

            if (!await checkNameAndBrand(data))
            {
                LogError("缺少美名和祝福技能");
                return false;
            }

            // 判断是否需要制作收藏品
            if (CollectValue > 0)
            {
                if (!Core.Player.HasAura(903))
                {
                    Actionmanager.DoAction("Collectable Synthesis", Core.Me);
                    await Coroutine.Sleep(250);
                }
            }
            else
            {
                // 判断是否有收藏品制作BUFF，如果有，则关闭
                if (Core.Player.HasAura(903))
                {
                    Actionmanager.DoAction("Collectable Synthesis", Core.Me);
                    await Coroutine.Sleep(250);
                }
            }

            // 下面开始制作
            await doAction("Comfort Zone"); //安逸
            await doAction("Inner Quiet"); //内静
            await doAction("Steady Hand II"); //稳手II
            await doAction("Waste Not II"); //简约II

            await doBestTouch();    // 加工
            await doBestTouch();    // 加工
            await doBestTouch();    // 加工
            await doBestTouch();    // 加工

            await doAction("Steady Hand II"); //稳手II

            await doBestTouch();    // 加工
            await doBestTouch();    // 加工
            await doBestTouch();    // 加工

            await doAction("Great Strides");    // 阔步

            if (CraftingManager.Condition == CraftingCondition.Excellent || CraftingManager.Condition == CraftingCondition.Good)
            {
                await doAction("Byregot's Brow");   //比尔格的技巧
                await doAction("Steady Hand II"); //稳手II
            }
            else
            {
                await doAction("Steady Hand II"); //稳手II

                if (CraftingManager.Condition == CraftingCondition.Excellent || CraftingManager.Condition == CraftingCondition.Good)
                {
                    await doAction("Byregot's Brow");   //比尔格的技巧
                }
                else
                {
                    await doAction("Innovation");   // 改革

                    if (CraftingManager.Condition == CraftingCondition.Excellent || CraftingManager.Condition == CraftingCondition.Good)
                    {
                        await doAction("Byregot's Brow");   //比尔格的技巧
                    }
                    else
                    {
                        await doAction("Byregot's Blessing");   // 比尔格的祝福
                    }
                }
            }

            await doName(data); //美名
            await doBrand(data);    //祝福

            if (CraftingManager.IsCrafting && CraftingManager.ProgressRequired != CraftingManager.Progress)
            {
                await doBrand(data);    //祝福
            }
            if (CraftingManager.IsCrafting && CraftingManager.ProgressRequired != CraftingManager.Progress)
            {
                await doFinishAction("Careful Synthesis II");// 模范制作II
            }

            if (SelectYesNoItem.IsOpen)
            {
                if (SelectYesNoItem.CollectabilityValue >= CollectValue)
                {
                    SelectYesNoItem.Yes();
                }
                else
                {
                    SelectYesNoItem.No();
                }
                await Coroutine.Wait(10000, () => !SelectYesNoItem.IsOpen);
                await Coroutine.Wait(Timeout.Infinite, () => !CraftingManager.AnimationLocked);
                await Coroutine.Sleep(250);
            }

            Log("制作结束");

            return isDone = true;
        }

        private async Task<bool> doFinishAction(string name)
        {
            Actionmanager.DoAction(name, Core.Me);

            await Coroutine.Wait(10000, () => CraftingManager.AnimationLocked);
            await Coroutine.Wait(Timeout.Infinite, () => !CraftingManager.AnimationLocked || ff14bot.RemoteWindows.SelectYesNoItem.IsOpen);
            await Coroutine.Sleep(250);

            return true;
        }

        private async Task<bool> doAction(uint aid)
        {
            Actionmanager.DoAction(aid, Core.Me);

            await Coroutine.Wait(10000, () => CraftingManager.AnimationLocked);
            await Coroutine.Wait(Timeout.Infinite, () => !CraftingManager.AnimationLocked);
            await Coroutine.Sleep(250);

            return true;
        }
        private async Task<bool> doAction(string aname)
        {
            Actionmanager.DoAction(aname, Core.Me);

            await Coroutine.Wait(10000, () => CraftingManager.AnimationLocked);
            await Coroutine.Wait(Timeout.Infinite, () => !CraftingManager.AnimationLocked);
            await Coroutine.Sleep(250);

            return true;
        }

        private async Task<bool> doBestTouch()
        {
            if (CraftingManager.Condition == CraftingCondition.Good || CraftingManager.Condition == CraftingCondition.Excellent)
            {
                await doAction("Precise Touch");
            }
            else
            {
                await doAction("Basic Touch");
            }
            return true;
        }

        // 美名
        private async Task<bool> doName(SpellData data)
        {
            if (Actionmanager.CurrentActions.TryGetValue("Name of Water", out data))
            {
                // 水之美名
                await doAction("Name of Water");
            }
            else if (Actionmanager.CurrentActions.TryGetValue("Name of the Wind", out data))
            {
                // 风之美名
                await doAction("Name of the Wind");
            }
            else if (Actionmanager.CurrentActions.TryGetValue("Name of Fire", out data))
            {
                // 火之美名
                await doAction("Name of Fire");
            }
            else if (Actionmanager.CurrentActions.TryGetValue("Name of Ice", out data))
            {
                // 冰之美名
                await doAction("Name of Ice");
            }
            else if (Actionmanager.CurrentActions.TryGetValue("Name of Earth", out data))
            {
                // 土之美名
                await doAction("Name of Earth");
            }
            else if (Actionmanager.CurrentActions.TryGetValue("Name of Lightning", out data))
            {
                // 雷之美名
                await doAction("Name of Lightning");
            }
            return true;
        }

        // 祝福
        private async Task<bool> doBrand(SpellData data)
        {
            if (Actionmanager.CurrentActions.TryGetValue("Brand of Water", out data))
            {
                // 水之祝福
                await doFinishAction("Brand of Water");
            }
            else if (Actionmanager.CurrentActions.TryGetValue("Brand of Wind", out data))
            {
                // 风之祝福
                await doFinishAction("Brand of Wind");
            }
            else if (Actionmanager.CurrentActions.TryGetValue("Brand of Fire", out data))
            {
                // 火之祝福
                await doFinishAction("Brand of Fire");
            }
            else if (Actionmanager.CurrentActions.TryGetValue("Brand of Ice", out data))
            {
                // 冰之祝福
                await doFinishAction("Brand of Ice");
            }
            else if (Actionmanager.CurrentActions.TryGetValue("Brand of Earth", out data))
            {
                // 土之祝福
                await doFinishAction("Brand of Earth");
            }
            else if (Actionmanager.CurrentActions.TryGetValue("Brand of Lightning", out data))
            {
                // 雷之祝福
                await doFinishAction("Brand of Lightning");
            }
            return true;
        }


        private async Task<bool> checkSkills(SpellData data)
        {
            return await checkSkill("Comfort Zone", "缺少技能：安逸", data)
                && await checkSkill("Inner Quiet", "缺少技能：内静", data)
                && await checkSkill("Steady Hand II", "缺少技能：稳手II", data)
                && await checkSkill("Waste Not II", "缺少技能：简约II", data)
                && await checkSkill("Careful Synthesis II", "缺少技能：模范制作II", data)
                && await checkSkill("Byregot's Brow", "缺少技能：比尔格的技巧", data)
                && await checkSkill("Great Strides", "缺少技能：阔步", data)
                && await checkSkill("Innovation", "缺少技能：改革", data)
                && await checkSkill("Byregot's Blessing", "缺少技能：比尔格的祝福", data);
        }

        private async Task<bool> checkSkill(string name, string errmsg, SpellData data)
        {

            if (!Actionmanager.CurrentActions.TryGetValue(name, out data))
            {
                if (!string.IsNullOrEmpty(errmsg))
                    LogError("{0}", errmsg);
                return false;
            }

            return true;
        }

        // 检查美名和祝福技能
        private async Task<bool> checkNameAndBrand(SpellData data)
        {
            return (await checkSkill("Brand of Water", null, data) && await checkSkill("Name of Water", null, data))
                || (await checkSkill("Brand of Wind", null, data) && await checkSkill("Name of the Wind", null, data))
                || (await checkSkill("Brand of Fire", null, data) && await checkSkill("Name of Fire", null, data))
                || (await checkSkill("Brand of Ice", null, data) && await checkSkill("Name of Ice", null, data))
                || (await checkSkill("Brand of Earth", null, data) && await checkSkill("Name of Earth", null, data))
                || (await checkSkill("Brand of Lightning", null, data) && await checkSkill("Name of Lightning", null, data));
        }
    }
}