using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExBuddy.Localization
{
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using ff14bot.Helpers;


    /// <summary>
    /// This jiggery is required due to how hb/rb/etc handle the resource system.
    /// </summary>
    public static class LocalizationInitializer
    {

        internal static bool Initialized = false;

        private static void AddLocalizedResourcesFromAssembly(ResourceManager resourceMgr)
        {
            AddLocalizedResource(resourceMgr, "zh-CN");
        }

        private static void AddLocalizedResource(ResourceManager resourceMgr, string cultureName)
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("ExBuddy.Localization.Localization." + cultureName + ".resources"))
            {
                if (s == null)
                {
                    Logging.WriteDiagnostic("Couldn't find {0}", "ExBuddy.Localization.Localization." + cultureName + ".resources");
                    return;
                }

                FieldInfo resourceSetsField = typeof(ResourceManager).GetField("_resourceSets", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
                Dictionary<string, ResourceSet> resourceSets = (Dictionary<string, ResourceSet>)resourceSetsField.GetValue(resourceMgr);

                ResourceSet resources = new ResourceSet(s);
                resourceSets.Add(cultureName, resources);
            }
        }

        public static void Initalize()
        {
            if (!Initialized)
            {
                AddLocalizedResourcesFromAssembly(Localization.ResourceManager);
            }
        }
    }
}
