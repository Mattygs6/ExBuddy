namespace ExBuddy.Skywatcher
{
    using System.Collections.Generic;

    public interface IWeatherProvider
    {
        IEnumerable<WeatherData> CurrentWeatherData { get; }

        IList<WeatherData> WeatherData { get; }

        void Disable();

        void Enable();
    }
}