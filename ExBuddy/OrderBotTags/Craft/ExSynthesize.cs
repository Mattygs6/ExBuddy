namespace ExBuddy.OrderBotTags.Craft
{
    using Clio.XmlEngine;

    using ExBuddy.Attributes;
    using ff14bot.NeoProfiles.Tags;
    using Providers;
    [LoggerName("ExSynthesize")]
    [XmlElement("ExSynthesize")]
    public sealed class ExSynthesize : Synthesize
    {
        [XmlAttribute("Name")]
        private string Name { set; get; }

        protected override void OnStart()
        {
            if(base.RecipeId == 0 && Name != null)
            {
                Log("加载前配方ID：" + base.RecipeId);
                uint id = RecipeProvider.Instance.GetRecipeIdByEnName(Name);
                Log("通过名称({0})获取配方ID：{1}",Name,id);
                if(id > 0)
                {
                    base.RecipeId = id;
                }
            }
        }
    }
}