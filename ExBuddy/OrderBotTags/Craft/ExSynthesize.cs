namespace ExBuddy.OrderBotTags.Craft
{
    using Buddy.Coroutines;
    using Clio.XmlEngine;

    using ExBuddy.Attributes;
    using ff14bot;
    using ff14bot.Behavior;
    using ff14bot.Managers;
    using ff14bot.NeoProfiles.Tags;
    using Providers;
    using System.Threading.Tasks;
    using System.Xml;
    using TreeSharp;

    [LoggerName("ExSynthesize")]
    [XmlElement("ExSynthesize")]
    public sealed class ExSynthesize : Synthesize
    {
        [XmlAttribute("Name")]
        private string Name { set; get; }

        [XmlAttribute("CnName")]
        private string CnName { set; get; }

        protected override void OnStart()
        {
            
            if (base.RecipeId == 0)
            {
                Log("加载前配方ID：" + base.RecipeId);


                XmlElement data = RecipeProvider.Instance.data;
                string xpath = null;

                if(Name != null)
                {
                    xpath = string.Format("/Recipes/Recipe[@Job='{0}']/recipe[@enname='{1}']/@id", Core.Me.CurrentJob, Name);
                } else if(CnName != null)
                {
                    xpath = string.Format("/Recipes/Recipe[@Job='{0}']/recipe[@cnname='{1}']/@id", Core.Me.CurrentJob, CnName);
                }

                if(xpath != null)
                {
                    Log("xpath:{0}", xpath);

                    XmlNode element = data.SelectSingleNode(xpath);
                    if (element != null)
                    {
                        Log("找到一个节点：{0}", element.Value);

                        base.RecipeId = uint.Parse(element.Value);
                    }
                    else
                    {
                        Log("没有找到节点");
                    }
                }
            }

            base.OnStart();
        }

        protected override Composite CreateBehavior()
        {
            return new ActionRunCoroutine(ctx => doExStart());
        }

        private async Task<bool> doExStart()
        {
            bool flag = true;
            flag = await Land() && await base.StartCrafting();
            return flag;
        }

        private async Task<bool> Land()
        {
            
            var result = true;
            if (MovementManager.IsFlying)
            {
                Log("处于飞行状态，准备降落");
                result = await CommonTasks.Land();
                await Coroutine.Sleep(1000);
            }

            if (result && Core.Player.IsMounted)
            {
                Log("处于骑乘状态，准备去掉骑乘");
                await CommonTasks.StopAndDismount();
                await Coroutine.Sleep(1000);
            }

            return result;
        }
        
    }
}