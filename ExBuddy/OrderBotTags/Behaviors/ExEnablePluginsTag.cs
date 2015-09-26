namespace ExBuddy.OrderBotTags.Behaviors
{
	using System;
	using System.Linq;
	using System.Windows.Media;

	using Clio.XmlEngine;

	using ExBuddy.Attributes;

	using ff14bot.Managers;

	[LoggerName("ExEnablePlugins")]
	[XmlElement("ExEnablePlugins")]
	public sealed class ExEnablePluginsTag : ExProfileBehavior
	{
		[XmlAttribute("Names")]
		public string[] Names { get; set; }

		protected override Color Info
		{
			get
			{
				return Colors.GreenYellow;
			}
		}

		protected override void OnStart()
		{
			if (Names == null || Names.Length == 0)
			{
				isDone = true;
				return;
			}

			StatusText = "Enabling Plugins: " + string.Join(", ", Names);
			Logger.Info("Enabling Plugins: " + string.Join(", ", Names));
			foreach (
				var plugin in
					PluginManager.Plugins.Where(p => Names.Contains(p.Plugin.Name, StringComparer.InvariantCultureIgnoreCase)))
			{
				try
				{
					if (plugin.Enabled)
					{
						Logger.Info("Plugin {0} already enabled.", plugin.Plugin.Name);
					}
					else
					{
						Logger.Info("Enabling Plugin {0}", plugin.Plugin.Name);
						plugin.Enabled = true;
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
