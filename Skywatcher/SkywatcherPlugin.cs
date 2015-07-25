namespace ExBuddy.Skywatcher
{
    using System;
    using System.Linq;

    using ExBuddy.Skywatcher.FF14Angler;

    using ff14bot.Interfaces;
    using ff14bot.Managers;

    public class SkywatcherPlugin : IBotPlugin
    {
        // 1 eorzea hour > 175 seconds > 2.91 minutes
        public const double RefreshRate = 3600 * 1000 * (7.0 / 144.0);

        public static readonly DateTime EorzeaStartTime = new DateTime(2010, 7, 13);

        public static bool IsEnabled { get; private set; }

        public static IWeatherProvider WeatherProvider { get; private set; }

        public static int GetIntervalNumber()
        {
            //JP differential (8 not 9?..we are matching angler using this calculation) -1 ? + 1 ?
            var interval = ((DateTime.UtcNow.AddHours(8) - EorzeaStartTime).TotalSeconds / 1400);

            return Convert.ToInt32(interval);
        }

        public static TimeSpan GetEorzeaTimeTillNextInterval()
        {
            var timeOfDay = WorldManager.EorzaTime.TimeOfDay;

            var secondsLeft = 60 - timeOfDay.Seconds;
            var minutesLeft = 60 - timeOfDay.Minutes + (secondsLeft == 0 ? 0 : -1);
            var hoursLeft = 8 - (timeOfDay.Hours % 8) + (minutesLeft == 0 && secondsLeft == 0 ? 0 : -1);

            var timeleft = new TimeSpan(hoursLeft, minutesLeft, secondsLeft);

            return timeleft;
        }

        public static double GetTimeTillNextInterval()
        {
            var timeOfDay = WorldManager.EorzaTime.TimeOfDay;

            var secondsLeft = 60 - timeOfDay.Seconds;
            var minutesLeft = 60 - timeOfDay.Minutes + (secondsLeft == 0 ? 0 : -1);
            var hoursLeft = 8 - (timeOfDay.Hours % 8) + (minutesLeft == 0 && secondsLeft == 0 ? 0 : -1);

            var timeLeft = (secondsLeft * 1000 + minutesLeft * 60 * 1000 + hoursLeft * 3600 * 1000) * (7.0 / 144.0);
            return timeLeft;
        }

        public static bool IsWeatherInZone(int zoneId, params byte[] weatherIds)
        {
            return
                WeatherProvider.CurrentWeatherData.Any(
                    w => w.ZoneId == zoneId && weatherIds.Any(wid => wid == w.WeatherId));
        }

        public static bool IsWeatherInZone(int zoneId, params string[] weatherNames)
        {
            return
                WeatherProvider.CurrentWeatherData.Any(
                    w =>
                    w.ZoneId == zoneId
                    && weatherNames.Any(wn => string.Equals(wn, w.Weather, StringComparison.InvariantCultureIgnoreCase)));
        }

        public static string GetWeatherNameById(byte weatherId)
        {
            string weatherName;
            WorldManager.WeatherDictionary.TryGetValue(weatherId, out weatherName);
            return weatherName;
        }

        public static bool IsWeather(byte weatherId)
        {
            return weatherId == WorldManager.CurrentWeatherId;
        }

        public static bool IsWeather(string weatherName)
        {
            return string.Equals(weatherName, WorldManager.CurrentWeather, StringComparison.InvariantCultureIgnoreCase);
        }

        #region IBotPlugin

        public bool Equals(IBotPlugin other)
        {
            return other.Name == this.Name;
        }

        public void OnButtonPress()
        {
            // TODO: Bring up timetable
        }

        public void OnPulse()
        {
            // TODO: probably nothing
        }

        public void OnInitialize()
        {
            WeatherProvider = new FF14AnglerWeatherProvider();
        }

        public void OnShutdown()
        {
            WeatherProvider = null;
        }

        public void OnEnabled()
        {
            IsEnabled = true;
            WeatherProvider.Enable();
        }

        public void OnDisabled()
        {
            IsEnabled = false;
            WeatherProvider.Disable();
            // TODO: if timetable implemented, close window
        }

        public string Author
        {
            get
            {
                return "ExMatt";
            }
        }

        public Version Version
        {
            get
            {
                return new Version(1, 0);
            }
        }

        public string Name
        {
            get
            {
                return "Skywatcher";
            }
        }

        public string Description
        {
            get
            {
                return "";
            }
        }

        public bool WantButton
        {
            get
            {
                return true;
            }
        }

        public string ButtonText
        {
            get
            {
                return "Mooo!";
            }
        }

        #endregion
    }
}