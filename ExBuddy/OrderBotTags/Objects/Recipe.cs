using System;

namespace ExBuddy.OrderBotTags.Objects
{
    /// <summary>
    /// Contains the recipe ID (taken from xivdb.com) and the name.
    /// </summary>
    public class Recipe
    {
        public uint Id { get; private set; }
        public string Name { get; private set; }
        public string CN_Name
        {
            get;
            private set; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Recipe ID from xivdb.com</param>
        /// <param name="name">Recipe Name</param>
        internal Recipe(uint id, string name
            //,string cn_name
            )
        {
            Id = id;
            Name = name;
            //CN_Name = cn_name;
        }

        /*
        public bool IsEqual(Recipe extRecipe)
        {
            return (extRecipe.Id == Id && extRecipe.Name == Name);
        }

        public bool IsSimilar(Recipe extRecipe)
        {
            if (!extRecipe.IsValid())
                return false;

            bool isEqual = string.Equals(extRecipe.Name, Name, StringComparison.CurrentCultureIgnoreCase);
            bool isSimilarName = string.Equals(extRecipe.Name, Name, StringComparison.CurrentCultureIgnoreCase);
            bool isSimilarNoDash = string.Equals(extRecipe.Name.Replace("-", " "),
                                                 Name.Replace("-", " "),
                                                 StringComparison.CurrentCultureIgnoreCase);
            bool containsString = Name.StartsWith(extRecipe.Name);

            if(isEqual)
                return true;

            if(isSimilarName || isSimilarNoDash ||  containsString)
            {
                return true;
            }
            return false;
        }
        */

        /// <summary>
        /// Checks if the passed recipe name (string) is similar to the object's name.
        /// </summary>
        /// <param name="extRecipeName"></param>
        /// <returns></returns>
        public bool IsSimilar(string extRecipeName)
        {
            if(string.IsNullOrEmpty(extRecipeName))
                return false;

            // Bool conditions which check if the passed recipe name is similar.
            bool isEqual = string.Equals(extRecipeName, Name, StringComparison.CurrentCultureIgnoreCase);
            bool isSimilarName = string.Equals(extRecipeName, Name, StringComparison.CurrentCultureIgnoreCase);
            bool isSimilarNoDash = string.Equals(extRecipeName.Replace("-", " "),
                                                 Name.Replace("-", " "),
                                                 StringComparison.CurrentCultureIgnoreCase);
            bool containsString = Name.StartsWith(extRecipeName);

            if(isEqual)
                return true;
            if(isSimilarName || isSimilarNoDash || containsString)
            {
                return true;
            }
            return false;
        }

        public bool IsValid()
        {
            return (!string.IsNullOrEmpty(Name) && Id > 0);
        }
    }
}

