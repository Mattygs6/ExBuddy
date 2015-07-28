namespace ExBuddy.Skywatcher.FF14Angler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Media;

    using ExBuddy.Core;

    using ff14bot.Helpers;
    using ff14bot.Managers;

    public class FF14AnglerWeatherProvider : IWeatherProvider
    {
        private static readonly Regex WeatherTitleRegex = new Regex(
            @"<(img)\b[^>]*title='(.*)'>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static int lastInterval;

        private static readonly object Locker = new object();

        private static readonly Timer RequestTimer = new Timer(GetEntries);

        private static IList<WeatherResult> weatherResults;

        private static readonly IDictionary<uint, uint> zoneMap = new Dictionary<uint, uint>
                                                                      {
                                                                          { 1, 129 },
                                                                          { 2, 134 },
                                                                          { 3, 135 },
                                                                          { 4, 137 },
                                                                          { 5, 138 },
                                                                          { 6, 139 },
                                                                          { 7, 180 },
                                                                          { 8, 250 },
                                                                          { 9, 339 }, // Mist
                                                                          { 10, 132 },
                                                                          { 11, 148 },
                                                                          { 12, 152 },
                                                                          { 13, 153 },
                                                                          { 14, 154 },
                                                                          { 15, 340 },
                                                                          // Lavender Beds
                                                                          { 16, 130 },
                                                                          { 17, 140 },
                                                                          { 18, 141 },
                                                                          { 19, 145 },
                                                                          { 20, 146 },
                                                                          { 21, 147 },
                                                                          { 22, 341 }, // Goblet
                                                                          { 25, 418 }, // Ishgard
                                                                          { 23, 155 }, // CCH
                                                                          { 26, 397 }, // CWH
                                                                          { 27, 401 },
                                                                          { 28, 402 },
                                                                          { 29, 478 },
                                                                          { 30, 398 },
                                                                          { 31, 399 },
                                                                          // Hinterlands
                                                                          { 32, 400 },
                                                                          { 24, 156 } // Mor Dhona
                                                                      };

        public bool IsEnabled { get; private set; }

        public void Enable()
        {
            lock (Locker)
            {
                if (!IsEnabled)
                {
                    IsEnabled = true;
                    RequestTimer.Change(0, (int)SkywatcherPlugin.GetTimeTillNextInterval());
                }
            }
        }

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

        public IEnumerable<WeatherData> CurrentWeatherData
        {
            get
            {
                return
                    weatherResults.Where(w => w.Time == 0)
                        .Select(
                            s =>
                            new WeatherData
                                {
                                    Time = s.Time,
                                    ZoneId = zoneMap[s.Area],
                                    Html = s.Html,
                                    Weather = GetTitleFromHtmlImg(s.Html),
                                    WeatherId = s.Weather
                                });
            }
        }

        public IList<WeatherData> WeatherData
        {
            get
            {
                return
                    weatherResults.Select(
                        s =>
                        new WeatherData
                            {
                                Time = s.Time,
                                ZoneId = zoneMap[s.Area],
                                Html = s.Html,
                                Weather = GetTitleFromHtmlImg(s.Html),
                                WeatherId = s.Weather
                            }).ToArray();
            }
        }

        /// <summary>
        ///     Gets the entries.
        /// </summary>
        /// <param name="stateInfo">The state info.</param>
        private static void GetEntries(object stateInfo)
        {
            if (WorldManager.EorzaTime.TimeOfDay.Hours % 8 == 0 || weatherResults == null
                || lastInterval < SkywatcherPlugin.GetIntervalNumber())
            {
                HttpClient client = null;
                try
                {
                    client = new HttpClient();
                    var response =
                        client.GetContentAsync<WeatherResponse>("http://en.ff14angler.com/skywatcher.php").Result;
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
                        RequestTimer.Change(TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds((int)SkywatcherPlugin.GetTimeTillNextInterval()));
                    }
                }
                catch (Exception ex)
                {
                    Logging.Write(Colors.Red, ex.Message);
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
            var matches = WeatherTitleRegex.Matches(htmlString);

            var match = WeatherTitleRegex.Match(htmlString);
            if (match.Success)
            {
                return match.Groups[2].Value;
            }

            return "Parse Failure";
        }
    }
}