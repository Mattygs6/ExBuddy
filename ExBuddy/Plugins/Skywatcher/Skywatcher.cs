namespace ExBuddy.Plugins.Skywatcher
{
	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;

	[LoggerName("Skywatcher")]
	public class Skywatcher : ExBotPlugin<Skywatcher>
	{
		// 1 eorzea hour > 175 seconds > 2.91 minutes
		public const double RefreshRate = 3600*1000*(7.0/144.0);

		public static IWeatherProvider WeatherProvider { get; private set; }

		#region IBotPlugin

		public override void OnButtonPress()
		{
			// TODO: Bring up timetable
		}

		public override void OnInitialize()
		{
			Skywatcher.WeatherProvider = new SaintCoinachWeatherProvider();
		}

		public override void OnShutdown()
		{
			Skywatcher.WeatherProvider = null;
		}

		public override void OnEnabled()
		{
			Skywatcher.WeatherProvider.Enable();
		}

		public override void OnDisabled()
		{
			Skywatcher.WeatherProvider.Disable();
			// TODO: if timetable implemented, close window
		}

		public override string Name
		{
			get { return Localization.Localization.Skywatcher_PluginName; }
		}

		#endregion
	}
}