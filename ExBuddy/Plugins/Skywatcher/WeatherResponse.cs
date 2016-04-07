namespace ExBuddy.Plugins.Skywatcher
{
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public class WeatherResponse
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="WeatherResponse" /> class.
		/// </summary>
		public WeatherResponse()
		{
			Data = new List<WeatherResult>();
		}

		/// <summary>
		///     Gets or sets the data
		/// </summary>
		/// <value>The data.</value>
		[JsonProperty("data")]
		public IList<WeatherResult> Data { get; set; }

		[JsonProperty("hour")]
		public int Hour { get; set; }

		[JsonProperty("left_hour")]
		public int HoursLeft { get; set; }

		[JsonProperty("interval")]
		public int Interval { get; set; }

		[JsonProperty("minute")]
		public int Minute { get; set; }

		[JsonProperty("left_minute")]
		public int MinutesLeft { get; set; }
	}
}