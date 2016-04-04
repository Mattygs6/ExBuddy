namespace ExBuddy.Providers
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;
	using ff14bot.Managers;

	public class MasterPieceSupplyDataProvider
	{
#if RB_CN
		private const string MsdFileName = "msd_cn.xml";
#else
		private const string MsdFileName = "msd.xml";
#endif

		public static readonly string DataFilePath;

		public static readonly MasterPieceSupplyDataProvider Instance;

		private readonly XDocument data;

		static MasterPieceSupplyDataProvider()
		{
			var path = Path.Combine(Environment.CurrentDirectory, "Plugins\\ExBuddy\\Data\\" + MsdFileName);

			if (File.Exists(path))
			{
				DataFilePath = path;
			}
			else
			{
				DataFilePath =
					Directory.GetFiles(PluginManager.PluginDirectory, "*" + MsdFileName, SearchOption.AllDirectories).FirstOrDefault();
			}

			Instance = new MasterPieceSupplyDataProvider(DataFilePath);
		}

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
			get { return data != null; }
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
#if RB_CN
				return 103 - index;
#else
				return 103 - index;
#endif
			}

			return null;
		}
	}
}