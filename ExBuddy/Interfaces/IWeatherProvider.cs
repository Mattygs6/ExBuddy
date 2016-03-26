namespace ExBuddy.Interfaces
{
	using System;

	public interface IWeatherProvider
	{
		int? GetCurrentWeatherByZone(int zoneId);

		int? GetForecastByZone(int zoneId, TimeSpan timeSpan);

		void Disable();

		void Enable();
	}
}