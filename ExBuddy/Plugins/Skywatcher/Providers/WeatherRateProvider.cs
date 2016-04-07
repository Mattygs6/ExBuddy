namespace ExBuddy.Plugins.Skywatcher.Providers
{
	using System;
	using System.IO;
	using System.Linq;
	using ExBuddy.Plugins.Skywatcher.Objects;
	using ff14bot.Managers;
	using Newtonsoft.Json;

	internal class WeatherRateProvider
	{
		private const string WeatherRateIndexFileName = "weatherRateIndex.json";

		public static readonly string DataFilePath;

		public static readonly WeatherRateProvider Instance;

		private readonly WeatherRateIndex data;

		static WeatherRateProvider()
		{
			var path = Path.Combine(Environment.CurrentDirectory, "Plugins\\ExBuddy\\Data\\" + WeatherRateIndexFileName);

			if (File.Exists(path))
			{
				DataFilePath = path;
			}
			else
			{
				DataFilePath =
					Directory.GetFiles(PluginManager.PluginDirectory, "*" + WeatherRateIndexFileName, SearchOption.AllDirectories)
						.FirstOrDefault();
			}

			Instance = new WeatherRateProvider(DataFilePath);
		}

		public WeatherRateProvider(string filePath)
		{
			if (!File.Exists(filePath))
			{
				return;
			}

			using (var file = File.OpenText(filePath))
			{
				var serializer = new JsonSerializer();

				data = (WeatherRateIndex) serializer.Deserialize(file, typeof (WeatherRateIndex));
			}
		}

		public bool IsValid
		{
			get { return data != null; }
		}

		public WeatherRates GetWeatherRates(int weatherRate)
		{
			WeatherRates weatherRates;
			if (!data.TryGetValue(string.Concat(weatherRate), out weatherRates))
			{
				return null;
			}

			return weatherRates;
		}
	}
}