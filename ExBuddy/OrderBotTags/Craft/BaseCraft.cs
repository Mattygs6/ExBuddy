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
    using ff14bot.Enums;
    using ff14bot.Objects;
    using System.ComponentModel;
    using System.Collections.Generic;
    public abstract class BaseCraft : ExProfileBehavior
    {
        [DefaultValue(0)]
        [XmlAttribute("CollectValue")]
        public int CollectValue { get; set; }

        protected async Task<bool> doFinishAction(string name)
        {
            Actionmanager.DoAction(getActionIdByName(name), Core.Me);

            await Coroutine.Wait(10000, () => CraftingManager.AnimationLocked);
            await Coroutine.Wait(Timeout.Infinite, () => !CraftingManager.AnimationLocked || ff14bot.RemoteWindows.SelectYesNoItem.IsOpen);
            await Coroutine.Sleep(250);

            return true;
        }

        protected async Task<bool> doAction(uint aid)
        {
            Actionmanager.DoAction(aid, Core.Me);

            await Coroutine.Wait(10000, () => CraftingManager.AnimationLocked);
            await Coroutine.Wait(Timeout.Infinite, () => !CraftingManager.AnimationLocked);
            await Coroutine.Sleep(250);

            return true;
        }
        protected async Task<bool> doAction(string aname)
        {
            return await doAction(getActionIdByName(aname));
        }

        protected async Task<bool> doBestTouch()
        {
            if(CraftingManager.Condition == CraftingCondition.Good || CraftingManager.Condition == CraftingCondition.Excellent)
            {
                await doAction("Precise Touch");
            } else
            {
                await doAction("Basic Touch");
            }
            return true;
        }

        // 美名
        protected async Task<bool> doName(SpellData data)
        {
            if(Actionmanager.CurrentActions.TryGetValue("Name of Water",out data))
            {
                // 水之美名
                await doAction("Name of Water");
            } else if (Actionmanager.CurrentActions.TryGetValue("Name of the Wind", out data))
            {
                // 风之美名
                await doAction("Name of the Wind");
            } else if (Actionmanager.CurrentActions.TryGetValue("Name of Fire", out data))
            {
                // 火之美名
                await doAction("Name of Fire");
            } else if (Actionmanager.CurrentActions.TryGetValue("Name of Ice", out data))
            {
                // 冰之美名
                await doAction("Name of Ice");
            } else if (Actionmanager.CurrentActions.TryGetValue("Name of Earth", out data))
            {
                // 土之美名
                await doAction("Name of Earth");
            } else if (Actionmanager.CurrentActions.TryGetValue("Name of Lightning", out data))
            {
                // 雷之美名
                await doAction("Name of Lightning");
            }
            return true;
        }

        // 祝福
        protected async Task<bool> doBrand(SpellData data)
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
        
        protected async Task<bool> checkSkill(string name,string errmsg,SpellData data)
        {

            if (!Actionmanager.CurrentActions.TryGetValue(name, out data))
            {
                if(errmsg != null)
                {
                    LogError("{0}", errmsg);
                }
                return false;
            }

            return true;
        }

        // 检查美名和祝福技能
        protected async Task<bool> checkNameAndBrand(SpellData data)
        {
            return (await checkSkill("Brand of Water", null, data) && await checkSkill("Name of Water", null, data))
                || (await checkSkill("Brand of Wind", null, data) && await checkSkill("Name of the Wind", null, data))
                || (await checkSkill("Brand of Fire", null, data) && await checkSkill("Name of Fire", null, data))
                || (await checkSkill("Brand of Ice", null, data) && await checkSkill("Name of Ice", null, data))
                || (await checkSkill("Brand of Earth", null, data) && await checkSkill("Name of Earth", null, data))
                || (await checkSkill("Brand of Lightning", null, data) && await checkSkill("Name of Lightning", null, data));
        }

        protected uint getActionIdByName(string name)
        {
            if (crossClassActions.ContainsKey(Name))
            {
                return crossClassActions[Name];
            }
            else if (classActions.ContainsKey(Core.Player.CurrentJob) && classActions[Core.Player.CurrentJob].ContainsKey(Name))
            {
                return classActions[Core.Player.CurrentJob][Name];
            } else
            {
                return 0;
            }
        }

        static Dictionary<ClassJobType, Dictionary<string, uint>> classActions = new Dictionary<ClassJobType, Dictionary<string, uint>>() {
            {ClassJobType.Carpenter, new Dictionary<string, uint>() {
                {"Basic Synthesis", 100001},
                {"Basic Touch", 100002},
                {"Master's Mend", 100003},
                {"Steady Hand", 244},
                {"Inner Quiet", 252},
                {"Observe", 100010},
                {"Rumination", 276},
                {"Standard Touch", 100004},
                {"Great Strides", 260},
                {"Master's Mend II", 100005},
                {"Standard Synthesis", 100007},
                {"Brand of Wind", 100006},
                {"Advanced Touch", 100008},
                {"Byregot's Blessing", 100009},
                {"Byregot's Brow", 100120},
                {"Precise Touch", 100128},
                {"Name Of The Wind", 4568}
            }},{ClassJobType.Blacksmith, new Dictionary<string, uint>() {
                {"Basic Synthesis", 100015},
                {"Basic Touch", 100016},
                {"Master's Mend", 100017},
                {"Steady Hand", 245},
                {"Inner Quiet", 253},
                {"Observe", 100023},
                {"Ingenuity", 277},
                {"Standard Touch", 100018},
                {"Great Strides", 261},
                {"Master's Mend II", 100019},
                {"Standard Synthesis", 100021},
                {"Brand of Fire", 100020},
                {"Advanced Touch", 100022},
                {"Ingenuity II", 283},
                {"Byregot's Brow", 100121},
                {"Precise Touch", 100129},
                {"Name Of Fire", 4569}
            }},{ClassJobType.Armorer, new Dictionary<string, uint>() {
                {"Basic Synthesis", 100030},
                {"Basic Touch", 100031},
                {"Master's Mend", 100032},
                {"Steady Hand", 246},
                {"Inner Quiet", 254},
                {"Observe", 100040},
                {"Rapid Synthesis", 100033},
                {"Standard Touch", 100034},
                {"Great Strides", 262},
                {"Master's Mend II", 100035},
                {"Standard Synthesis", 100037},
                {"Brand of Ice", 100036},
                {"Advanced Touch", 100038},
                {"Piece by Piece", 100039},
                {"Byregot's Brow", 100122},
                {"Precise Touch", 100130},
                {"Name Of Ice", 4570}
            }},{ClassJobType.Goldsmith, new Dictionary<string, uint>() {
                {"Basic Synthesis", 100075},
                {"Basic Touch", 100076},
                {"Master's Mend", 100077},
                {"Steady Hand", 247},
                {"Inner Quiet", 255},
                {"Observe", 100082},
                {"Manipulation", 278},
                {"Standard Touch", 100078},
                {"Great Strides", 263},
                {"Master's Mend II", 100079},
                {"Standard Synthesis", 100080},
                {"Flawless Synthesis", 100083},
                {"Advanced Touch", 100081},
                {"Innovation", 284},
                {"Byregot's Brow", 100123},
                {"Precise Touch", 100131},
                {"Maker's Mark", 100178}
            }},{ClassJobType.Leatherworker, new Dictionary<string, uint>() {
                {"Basic Synthesis", 100045},
                {"Basic Touch", 100046},
                {"Master's Mend", 100047},
                {"Steady Hand", 249},
                {"Inner Quiet", 257},
                {"Observe", 100053},
                {"Waste Not", 279},
                {"Standard Touch", 100048},
                {"Great Strides", 265},
                {"Master's Mend II", 100049},
                {"Standard Synthesis", 100051},
                {"Brand of Earth", 100050},
                {"Advanced Touch", 100052},
                {"Waste Not II", 285},
                {"Byregot's Brow", 100124},
                {"Precise Touch", 100132},
                {"Name Of Earth", 4571}
            }},{ClassJobType.Weaver, new Dictionary<string, uint>() {
                {"Basic Synthesis", 100060},
                {"Basic Touch", 100061},
                {"Master's Mend", 100062},
                {"Steady Hand", 248},
                {"Inner Quiet", 256},
                {"Observe", 100070},
                {"Careful Synthesis", 100063},
                {"Standard Touch", 100064},
                {"Great Strides", 264},
                {"Master's Mend II", 100065},
                {"Standard Synthesis", 100067},
                {"Brand of Lightning", 100066},
                {"Advanced Touch", 100068},
                {"Careful Synthesis II", 100069},
                {"Byregot's Brow", 100125},
                {"Precise Touch", 100133},
                {"Name Of Lightning", 4572}
            }},{ClassJobType.Alchemist, new Dictionary<string, uint>() {
                {"Basic Synthesis", 100090},
                {"Basic Touch", 100091},
                {"Master's Mend", 100092},
                {"Steady Hand", 250},
                {"Inner Quiet", 258},
                {"Observe", 100099},
                {"Tricks of the Trade", 100098},
                {"Standard Touch", 100093},
                {"Great Strides", 266},
                {"Master's Mend II", 100094},
                {"Standard Synthesis", 100096},
                {"Brand of Water", 100095},
                {"Advanced Touch", 100097},
                {"Comfort Zone", 286},
                {"Byregot's Brow", 100126},
                {"Precise Touch", 100134},
                {"Name Of Water", 4573}
            }},{ClassJobType.Culinarian, new Dictionary<string, uint>() {
                {"Basic Synthesis", 100105},
                {"Basic Touch", 100106},
                {"Master's Mend", 100107},
                {"Steady Hand", 251},
                {"Inner Quiet", 259},
                {"Observe", 100113},
                {"Hasty Touch", 100108},
                {"Standard Touch", 100109},
                {"Great Strides", 267},
                {"Master's Mend II", 100110},
                {"Standard Synthesis", 100111},
                {"Steady Hand II", 281},
                {"Advanced Touch", 100112},
                {"Reclaim", 287},
                {"Byregot's Brow", 100127},
                {"Precise Touch", 100135},
                {"Muscle Memory", 100136}
            }}
        };

        static Dictionary<string, uint> crossClassActions = new Dictionary<string, uint>() {
            {"Rumination", 276},
            {"Byregot's Blessing", 100009},
            {"Brand of Wind", 100006},
            {"Ingenuity II", 283},
            {"Ingenuity", 277},
            {"Brand of Fire", 100020},
            {"Rapid Synthesis", 100033},
            {"Piece by Piece", 100039},
            {"Brand of Ice", 100036},
            {"Waste Not", 279},
            {"Waste Not II", 285},
            {"Brand of Earth", 100050},
            {"Careful Synthesis", 100063},
            {"Careful Synthesis II", 100069},
            {"Brand of Lightning", 100066},
            {"Tricks of the Trade", 100098},
            {"Brand of Water", 100095},
            {"Comfort Zone", 286},
            {"Hasty Touch", 100108},
            {"Reclaim", 287},
            {"Steady Hand II", 281},
            {"Manipulation", 278},
            {"Innovation", 284},
            {"Flawless Synthesis", 100083},
            {"Name Of The Wind", 4568},
            {"Name Of Fire", 4569},
            {"Name Of Ice", 4570},
            {"Name Of Earth", 4571},
            {"Name Of Lightning", 4572},
            {"Name Of Water", 4573},
            {"Muscle Memory", 100136},
            {"Maker's Mark", 100178}
        };
    }
}