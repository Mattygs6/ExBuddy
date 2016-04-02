namespace ExBuddy.Plugins.Skywatcher.Providers
{
	using System;
	using System.IO;
	using System.Linq;
	using ExBuddy.Plugins.Skywatcher.Objects;
	using ff14bot.Managers;
	using Newtonsoft.Json;

	public class LocationProvider
	{
		private const string LocationIndexFileName = "locationIndex.json";

		public static readonly string DataFilePath;

		public static readonly LocationProvider Instance;

		private readonly LocationIndex data;

		static LocationProvider()
		{
			var path = Path.Combine(Environment.CurrentDirectory, "Plugins\\ExBuddy\\Data\\" + LocationIndexFileName);

			if (File.Exists(path))
			{
				DataFilePath = path;
			}
			else
			{
				DataFilePath =
					Directory.GetFiles(PluginManager.PluginDirectory, "*" + LocationIndexFileName, SearchOption.AllDirectories)
						.FirstOrDefault();
			}

			Instance = new LocationProvider(DataFilePath);
		}

		public LocationProvider(string filePath)
		{
			if (!File.Exists(filePath))
			{
				return;
			}

			using (var file = File.OpenText(filePath))
			{
				var serializer = new JsonSerializer();

				data = (LocationIndex) serializer.Deserialize(file, typeof (LocationIndex));
			}
		}

		public bool IsValid
		{
			get { return data != null; }
		}

		public Location GetLocation(int subZoneId)
		{
			Location location;
			if (!data.TryGetValue(string.Concat(subZoneId), out location))
			{
				return null;
			}

			return location;
		}
	}
}