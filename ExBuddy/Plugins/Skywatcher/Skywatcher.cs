namespace ExBuddy.Plugins.Skywatcher
{
	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;

	[LoggerName("Skywatcher")]
	public class Skywatcher : ExBotPlugin<Skywatcher>
	{
		// 1 eorzea hour > 175 seconds > 2.91 minutes
		public const double RefreshRate = 3600 * 1000 * (7.0 / 144.0);

		public static IWeatherProvider WeatherProvider { get; private set; }

		#region IBotPlugin

		public override void OnButtonPress()
		{
			// TODO: Bring up timetable
		}

		public override void OnInitialize()
		{
			WeatherProvider = new FF14AnglerWeatherProvider();
		}

		public override void OnShutdown()
		{
			WeatherProvider = null;
		}

		public override void OnEnabled()
		{
			WeatherProvider.Enable();
		}

		public override void OnDisabled()
		{
			WeatherProvider.Disable();
			// TODO: if timetable implemented, close window
		}

		public override string Name
		{
			get
			{
				return "Skywatcher";
			}
		}

		#endregion
	}
}