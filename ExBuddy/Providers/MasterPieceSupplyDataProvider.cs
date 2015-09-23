namespace ExBuddy.Providers
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;

	using ff14bot.Managers;

	public class MasterPieceSupplyDataProvider
	{
		public static readonly string DataFilePath;

		public static readonly MasterPieceSupplyDataProvider Instance;

		static MasterPieceSupplyDataProvider()
		{
			var path = Path.Combine(Environment.CurrentDirectory, "Plugins\\ExBuddy\\Data\\msd.xml");

			if (File.Exists(path))
			{
				DataFilePath = path;
			}
			else
			{
				DataFilePath =
					Directory.GetFiles(PluginManager.PluginDirectory, "*msd.xml", SearchOption.AllDirectories).FirstOrDefault();
			}

			Instance = new MasterPieceSupplyDataProvider(DataFilePath);
		}

		private readonly XDocument data;

		public MasterPieceSupplyDataProvider(string filePath)
		{
			if (!File.Exists(filePath))
			{
				return;
			}

			data = XDocument.Load(filePath);
		}

		public bool IsValid
		{
			get
			{
				return data != null;
			}
		}

		public uint? GetIndexByItemName(string itemName)
		{
			if (data == null)
			{
				return null;
			}

			var result =
				data.Root.Descendants("MS")
					.FirstOrDefault(
						e => e.Elements().Any(c => string.Equals(c.Value, itemName, StringComparison.InvariantCultureIgnoreCase)));

			if (result == null)
			{
				return null;
			}

			uint index;
			// ReSharper disable once PossibleNullReferenceException
			if (uint.TryParse(result.Element("S").Value, out index))
			{
				return 80 - index;
			}

			return null;
		}
	}
}