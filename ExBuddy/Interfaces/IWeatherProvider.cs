namespace ExBuddy.Interfaces
{
	using System;

	public interface IWeatherProvider
	{
		void Disable();

		void Enable();

		int? GetCurrentWeatherByZone(int zoneId);

		int? GetForecastByZone(int zoneId, TimeSpan timeSpan);
	}
}