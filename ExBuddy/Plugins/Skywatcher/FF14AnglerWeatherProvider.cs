namespace ExBuddy.Plugins.Skywatcher
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ExBuddy.Logging;
	using ff14bot.Managers;

	public class FF14AnglerWeatherProvider : IWeatherProvider
	{
		private static readonly object Locker = new object();

		private static readonly Regex WeatherTitleRegex = new Regex(
			@"<(img)\b[^>]*title='(.*)'>",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private static int lastInterval;

		private static readonly Timer RequestTimer = new Timer(GetEntries);

		private static readonly IDictionary<int, int> ZoneMap = new Dictionary<int, int>
		{
			{129, 1},
			{134, 2},
			{135, 3},
			{137, 4},
			{138, 5},
			{139, 6},
			{180, 7},
			{250, 8},
			{339, 9}, // Mist
			{132, 10},
			{148, 11},
			{152, 12},
			{153, 13},
			{154, 14},
			{340, 15}, // Lavender Beds
			{130, 16},
			{140, 17},
			{141, 18},
			{145, 19},
			{146, 20},
			{147, 21},
			{341, 22}, // Goblet
			{418, 25}, // Ishgard
			{155, 23}, // CCH
			{397, 26}, // CWH
			{401, 27},
			{402, 28},
			{478, 29}, // Idyllshire
			{398, 30}, // Forelands
			{399, 31}, // Hinterlands
			{400, 32}, // Churning
			{156, 24} // Mor Dhona
		};

		private static IList<WeatherResult> weatherResults;

		public bool IsEnabled { get; private set; }

		/// <summary>
		///     Gets the entries.
		/// </summary>
		/// <param name="stateInfo">The state info.</param>
		private static void GetEntries(object stateInfo)
		{
			if (WorldManager.EorzaTime.TimeOfDay.Hours%8 == 0 || weatherResults == null
			    || lastInterval < SkywatcherPlugin.GetIntervalNumber())
			{
				HttpClient client = null;
				try
				{
					client = new HttpClient();
					var response = client.GetContentAsync<WeatherResponse>("http://en.ff14angler.com/skywatcher.php").Result;
					if (response.Interval > lastInterval)
					{
						// Ensure we at least have all of the entries for the current time.
						if (response.Data.Count(w => w.Time == 0) >= 32 || weatherResults == null)
						{
							lastInterval = response.Interval;
							weatherResults = response.Data;
						}
						// If there are 32 or more weather forecasts, shift all weather down an interval.
						else if (weatherResults.Count(w => w.Time == 1) >= 32)
						{
							foreach (var w in weatherResults)
							{
								w.Time--;
							}
						}
					}
					else
					{
						// New interval not posted, retry every 30 seconds
						RequestTimer.Change(
							TimeSpan.FromSeconds(30),
							TimeSpan.FromMilliseconds((int) SkywatcherPlugin.GetTimeTillNextInterval()));
					}
				}
				catch (Exception ex)
				{
					Logger.Instance.Error(ex.Message);
				}
				finally
				{
					if (client != null)
					{
						client.Dispose();
					}
				}
			}
		}

		private string GetTitleFromHtmlImg(string htmlString)
		{
			var match = WeatherTitleRegex.Match(htmlString);
			if (match.Success)
			{
				return match.Groups[2].Value;
			}

			return "Parse Failure";
		}

		#region IWeatherProvider Members

		public void Disable()
		{
			lock (Locker)
			{
				if (IsEnabled)
				{
					IsEnabled = false;
					RequestTimer.Change(-1, -1);
					weatherResults.Clear();
					weatherResults = null;
					lastInterval = 0;
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
					RequestTimer.Change(0, (int) SkywatcherPlugin.GetTimeTillNextInterval());
				}
			}
		}

		public int? GetCurrentWeatherByZone(int zoneId)
		{
			int ff14AnglerZoneId;
			if (!ZoneMap.TryGetValue(zoneId, out ff14AnglerZoneId))
			{
				return null;
			}

			var weather = weatherResults.FirstOrDefault(s => s.Time == 0 && s.Area == ff14AnglerZoneId);

			if (weather != null)
			{
				return (int) weather.Weather;
			}

			return null;
		}

		public int? GetForecastByZone(int zoneId, TimeSpan timeSpan)
		{
			int time;
			var etTillNextInterval = SkywatcherPlugin.GetEorzeaTimeTillNextInterval();

			if (timeSpan > etTillNextInterval.Add(TimeSpan.FromHours(8)))
			{
				time = 2;
			}
			else if (timeSpan > etTillNextInterval)
			{
				time = 1;
			}
			else
			{
				time = 0;
			}

			int ff14AnglerZoneId;
			if (!ZoneMap.TryGetValue(zoneId, out ff14AnglerZoneId))
			{
				return null;
			}

			var weather = weatherResults.FirstOrDefault(s => s.Time == time && s.Area == ff14AnglerZoneId);

			if (weather != null)
			{
				return (int) weather.Weather;
			}

			return null;
		}

		#endregion
	}
}