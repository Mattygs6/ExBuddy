namespace ExBuddy.Interfaces
{
	using System.Collections.Generic;

	using ExBuddy.Plugins.Skywatcher;

	public interface IWeatherProvider
	{
		IEnumerable<WeatherData> CurrentWeatherData { get; }

		IList<WeatherData> WeatherData { get; }

		void Disable();

		void Enable();
	}
}