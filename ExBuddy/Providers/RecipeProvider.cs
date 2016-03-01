namespace ExBuddy.Providers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    using ff14bot.Managers;
    using System.Collections.Generic;
    using ff14bot.Enums;
    using ff14bot;
    using OrderBotTags.Objects;
    public class RecipeProvider
    {
		public static readonly string DataFilePath;

		public static readonly RecipeProvider Instance;

        // 英文配方
        private Dictionary<ClassJobType, string> enRecipes;
        // 中文配方
        private Dictionary<ClassJobType, string> cnRecipes;


		static RecipeProvider()
		{
            Logging.Logger.Instance.Info("初始化配方");
			var path = Path.Combine(Environment.CurrentDirectory, "Plugins\\ExBuddy\\Data\\Recipes");

			if (File.Exists(path))
			{
                Logging.Logger.Instance.Info("路径存在");
                DataFilePath = path;
			} else
            {
                Logging.Logger.Instance.Info("路径不存在");
            }

			Instance = new RecipeProvider(DataFilePath);
		}

		public RecipeProvider(string filePath)
		{
			if (!File.Exists(filePath))
			{
				return;
			}

            Dictionary<ClassJobType, string> jobs = new Dictionary<ClassJobType, string>
            {
                {ClassJobType.Alchemist, "recipes_alchemist.txt"},
                {ClassJobType.Armorer, "recipes_armorer.txt"},
                {ClassJobType.Blacksmith, "recipes_blacksmith.txt"},
                {ClassJobType.Carpenter, "recipes_carpenter.txt"},
                {ClassJobType.Culinarian, "recipes_culinarian.txt"},
                {ClassJobType.Goldsmith, "recipes_goldsmith.txt"},
                {ClassJobType.Leatherworker, "recipes_leatherworker.txt"},
                {ClassJobType.Weaver, "recipes_weaver.txt"}
            };

            //Dictionary<ClassJobType, Directory> recipe;
            
        }

		public bool IsValid
		{
			get
			{
				return enRecipes != null || cnRecipes != null;
			}
		}

        public uint GetRecipeIdByEnName(string recipeName)
        {
            Logging.Logger.Instance.Info("获取配方{0}",recipeName);
            if (IsValid)
            {
                return 0;
            }

            string recipeList = enRecipes[Core.Me.CurrentJob];

            string jobPath = string.Format("{0}\\{1}", DataFilePath, recipeList);
            Logging.Logger.Instance.Info("当前配方地址{0}", jobPath);

            // Add the lines of the read file to the list variable.
            List<string> rawRecipeList = File.ReadLines(jobPath).Where(line => line != null).ToList();

            // Create a Recipe object for each item inside the rawRecipeList and store them in the recipes list.
            foreach (string recipe in rawRecipeList)
            {
                Recipe recipeObj = ParseRecipe(recipe);
                if (recipeObj != null)
                {
                    if (recipeObj.Name == recipeName)
                        return recipeObj.Id;
                }
            }

            return 0;
        }

        private Recipe ParseRecipe(string recipeRaw)
        {
            if (recipeRaw.IndexOf(':') == -1)
                return null;
            string[] recipeSplit = recipeRaw.Split(':');
            if (recipeSplit.Length != 2) return null;

            uint recipeId;
            string recipeName = recipeSplit[1].Substring(1);
            if (uint.TryParse(recipeSplit[0], out recipeId))
                return new Recipe(recipeId, recipeName.Replace("-", " "));

            return null;
        }
	}
}