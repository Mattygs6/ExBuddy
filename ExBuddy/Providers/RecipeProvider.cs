namespace ExBuddy.Providers
{
    using ff14bot.Enums;
    using ff14bot.Managers;
    using Logging;
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml;
    public class RecipeProvider
    {
		public static readonly string DataFilePath;

		public static readonly RecipeProvider Instance;
        
        public readonly XmlElement data;

        static RecipeProvider()
		{
            Logger.Instance.Info("初始化配方");

            var path = Path.Combine(Environment.CurrentDirectory, "Plugins\\ExBuddy\\Data\\recipes.xml");

            if (File.Exists(path))
            {
                DataFilePath = path;
            }
            else
            {
                DataFilePath =
                    Directory.GetFiles(PluginManager.PluginDirectory, "*recipes.xml", SearchOption.AllDirectories).FirstOrDefault();
            }

			Instance = new RecipeProvider(DataFilePath);
		}

		public RecipeProvider(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            data = doc.DocumentElement;

        }

		public bool IsValid
		{
			get
            {
                return data != null;
            }
		}

        public uint GetRecipeIdByEnName(string recipeName,ClassJobType job)
        {
            Logger.Instance.Info("获取配方{0}-{1}",job,recipeName);
            if (IsValid)
            {
                return 0;
            }

            string xpath = string.Format("/Recipes/Recipe[@Job='{0}']/recipe[@enname='{1}']/@id", job, recipeName);
            Logger.Instance.Info("xpath:{0}", xpath);

            XmlNode element = data.SelectSingleNode(xpath);

            if(element != null)
            {
                Logger.Instance.Info("找到节点：{0}", element.Value);
                return uint.Parse(element.Value);
            } else
            {
                Logger.Instance.Error("没有找到节点{0}-{1}", job, recipeName);
            }

            return 0;
        }
        
	}
}