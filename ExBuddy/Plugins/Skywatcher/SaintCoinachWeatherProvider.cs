namespace ExBuddy.Plugins.Skywatcher
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Clio.Utilities;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ExBuddy.Plugins.Skywatcher.Providers;
	using ff14bot.Managers;

	public class SaintCoinachWeatherProvider : IWeatherProvider
	{
		private static readonly object Locker = new object();

		private static readonly IDictionary<int, int> ZoneMap = new Dictionary<int, int>
		{
			{129, 27}, // Limsa Lominsa
			{134, 30}, // Middle LN
			{135, 31}, // Lower LN
			{137, 32}, // Eastern LN
			{138, 33}, // Western LN
			{139, 34}, // Upper LN
			{180, 350}, // Outer LN
			//{ 250, 8 }, // Wolves' Den Pier
			{339, 425}, // Mist
			{132, 39}, // Gridania
			{148, 54}, // Central Shroud
			{152, 55}, // East Shroud
			{153, 56}, // South Shroud
			{154, 57}, // North Shroud
			{340, 426}, // Lavender Beds
			{130, 51}, // Ul'dah
			{140, 42}, // Western Than
			{141, 43}, // Central Than
			{145, 44}, // Eastern Than
			{146, 45}, // Southern Than
			{147, 46}, // Northern Than
			{341, 427}, // Goblet
			{418, 62}, // Ishgard
			{155, 63}, // CCH
			{397, 2200}, // CWH
			{156, 67}, // Mor Dhona
			{401, 2100}, // Sea of Clouds
			{402, 2101}, // Azys Lla
			{-1, -1}, // Daidem Easy
			{-2, -2}, // Daidem
			{-3, -3}, // Daidem Hard
			{398, 2000}, // Drav Fore
			{399, 2001}, // Drav Hinterlands
			{400, 2002}, // Churning Mists
			{478, 2082} // Idyllshire
		};

		public bool IsEnabled { get; private set; }

		public byte CalculateRate(DateTime time)
		{
			var unixSeconds = Utilities.ConvertToUnixTimestamp(time);
			// Get Eorzea hour for weather start
			var bell = unixSeconds/175;

			// Do the magic 'cause for calculations 16:00 is 0, 00:00 is 8 and 08:00 is 16
			var increment = (bell + 8 - (bell%8))%24;

			// Take Eorzea days since unix epoch
			var eDays = unixSeconds/4200;
			var totalDays = (((uint) eDays) << 32) >> 0;

			// 0x64 = 100
			var calcBase = totalDays*100 + increment;

			// 0xB = 11
			var step1 = ((uint) ((calcBase << 11) ^ calcBase)) >> 0;
			var step2 = ((step1 >> 8) ^ step1) >> 0;

			// 0x64 = 100
			return (byte) (step2%100);
		}

		#region IWeatherProvider Members

		public void Disable()
		{
			lock (Locker)
			{
				if (IsEnabled)
				{
					IsEnabled = false;
				}
			}
		}

		public void Enable()
		{
			lock (Locker)
			{
				if (!IsEnabled)
				{
					IsEnabled = true;
				}
			}
		}

		public int? GetCurrentWeatherByZone(int zoneId)
		{
			var location = LocationProvider.Instance.GetLocation(ZoneMap[zoneId]);

			if (location == null)
			{
				return null;
			}

			var date = WorldManager.EorzaTime;
			var localDate = SkywatcherPlugin.EorzeaToLocal(date);

			var rate = CalculateRate(localDate);

			var weatherRates = WeatherRateProvider.Instance.GetWeatherRates(location.WeatherRate);

			if (weatherRates == null)
			{
				return null;
			}

			var weatherRate = weatherRates.Rates.FirstOrDefault(r => rate < r.Rate);

			if (weatherRate == null)
			{
				return null;
			}

			return weatherRate.Weather;
		}

		public int? GetForecastByZone(int zoneId, TimeSpan timeSpan)
		{
			var location = LocationProvider.Instance.GetLocation(ZoneMap[zoneId]);

			if (location == null)
			{
				return null;
			}

			var date = WorldManager.EorzaTime + timeSpan;
			var localDate = SkywatcherPlugin.EorzeaToLocal(date);

			var rate = CalculateRate(localDate);

			var weatherRates = WeatherRateProvider.Instance.GetWeatherRates(location.WeatherRate);

			if (weatherRates == null)
			{
				return null;
			}

			var weatherRate = weatherRates.Rates.FirstOrDefault(r => rate < r.Rate);

			if (weatherRate == null)
			{
				return null;
			}

			return weatherRate.Weather;
		}

		#endregion
	}
}