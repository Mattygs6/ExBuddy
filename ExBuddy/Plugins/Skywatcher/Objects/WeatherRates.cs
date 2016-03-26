namespace ExBuddy.Plugins.Skywatcher.Objects
{
	using System.Collections.Generic;

	public class WeatherRates
	{
		public int Id { get; set; }

		public List<WeatherRate> Rates { get; set; }
	}
}