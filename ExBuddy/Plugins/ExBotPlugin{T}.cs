namespace ExBuddy.Plugins
{
	using System;
	using System.Linq;
	using System.Windows.Media;
	using ExBuddy.Attributes;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ExBuddy.Logging;
	using ff14bot.AClasses;
	using ff14bot.Managers;
	using ff14bot.Objects;

	public abstract class ExBotPlugin<T> : BotPlugin, ILogColors
		where T : ExBotPlugin<T>
	{
		protected internal readonly Logger Logger;

		static ExBotPlugin()
		{
			ReflectionHelper.CustomAttributes<LoggerNameAttribute>.RegisterByAssembly();

			Condition.AddNamespacesToScriptManager("ExBuddy", "ExBuddy.Helpers");
		}

		protected ExBotPlugin()
		{
			Logger = new Logger(this);
		}

		public override string Author
		{
			get { return "ExMatt"; }
		}

		public static bool IsEnabled
		{
			get { return PluginManager.Plugins.Any(p => p.Plugin.GetType() == typeof (T)); }
		}

		public override Version Version
		{
			get { return Logger.Version; }
		}

		protected virtual Color Error
		{
			get { return Logger.Colors.Error; }
		}

		protected virtual Color Info
		{
			get { return Logger.Colors.Info; }
		}

		protected static LocalPlayer Me
		{
			get { return GameObjectManager.LocalPlayer; }
		}

		protected virtual Color Warn
		{
			get { return Logger.Colors.Warn; }
		}

		#region ILogColors Members

		Color ILogColors.Error
		{
			get { return Error; }
		}

		Color ILogColors.Info
		{
			get { return Info; }
		}

		Color ILogColors.Warn
		{
			get { return Warn; }
		}

		#endregion
	}
}