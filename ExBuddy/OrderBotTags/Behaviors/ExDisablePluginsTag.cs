#pragma warning disable 1998
namespace ExBuddy.OrderBotTags.Behaviors
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Media;

	using Clio.XmlEngine;

	using ExBuddy.Attributes;

	using ff14bot.Managers;

	[LoggerName("ExDisablePlugins")]
	[XmlElement("ExDisablePlugins")]
	public sealed class ExDisablePluginsTag : ExProfileBehavior
	{
		private IList<string> namesList;

		[XmlAttribute("Names")]
		public string Names { get; set; }

		protected override Color Info
		{
			get
			{
				return Colors.PaleGoldenrod;
			}
		}

		private IList<string> NamesList
		{
			get
			{
				if (Names == null)
				{
					return new string[] { };
				}

				return namesList ?? (namesList = Names.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray());
			}
		}

		protected override async Task<bool> Main()
		{
			return true;
		}

		protected override void OnStart()
		{
			if (NamesList == null || NamesList.Count == 0)
			{
				isDone = true;
				return;
			}

			StatusText = "Disabling Plugins: " + Names;
			Logger.Info("Disabling Plugins: " + Names);

			foreach (var plugin in
				PluginManager.Plugins.Where(p => NamesList.Contains(p.Plugin.Name, StringComparer.InvariantCultureIgnoreCase)))
			{
				try
				{
					if (!plugin.Enabled)
					{
						Logger.Info("Plugin {0} already disabled.", plugin.Plugin.Name);
					}
					else
					{
						Logger.Info("Disabling Plugin {0}", plugin.Plugin.Name);
						plugin.Enabled = false;
					}
				}
				catch (Exception ex)
				{
					Logger.Error(ex.Message);
				}
			}

			isDone = true;
		}
	}
}