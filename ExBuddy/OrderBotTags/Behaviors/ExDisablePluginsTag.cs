namespace ExBuddy.OrderBotTags.Behaviors
{
	using System;
	using System.Linq;
	using System.Windows.Media;

	using Clio.XmlEngine;

	using ExBuddy.Attributes;

	using ff14bot.Managers;

	[LoggerName("ExDisablePlugins")]
	[XmlElement("ExDisablePlugins")]
	public sealed class ExDisablePluginsTag : ExProfileBehavior
	{
		[XmlElement("Names")]
		public string[] Names { get; set; }

		protected override Color Info
		{
			get
			{
				return Colors.SandyBrown;
			}
		}

		protected override void OnStart()
		{
			if (Names == null || Names.Length == 0)
			{
				isDone = true;
				return;
			}

			StatusText = "Disabling Plugins: " + string.Join(", ", Names);
			Logger.Info("Disabling Plugins: " + string.Join(", ", Names));
			foreach (
				var plugin in
					PluginManager.Plugins.Where(p => Names.Contains(p.Plugin.Name, StringComparer.InvariantCultureIgnoreCase)))
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
