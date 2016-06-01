namespace ExBuddy.Data
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	using ff14bot.Managers;

	using SQLite;

	public class SqlData : SQLiteConnection
	{
#if RB_CN
		private const string DbFileName = "ExBuddy_CN.s3db";
#else
		private const string DbFileName = "ExBuddy.s3db";
#endif
		public static readonly string DataFilePath;

		public static readonly SqlData Instance;

		private readonly Dictionary<uint, MasterpieceSupplyDutyResult> masterpieceSupplyDutyCache;

		// TODO: look into what localizedDictionary does for us??
		private readonly Dictionary<uint, RequiredItemResult> requiredItemCache;

		static SqlData()
		{
			var path = Path.Combine(Environment.CurrentDirectory, "Plugins\\ExBuddy\\Data\\" + DbFileName);

			if (File.Exists(path))
			{
				DataFilePath = path;
			}
			else
			{
				DataFilePath =
					Directory.GetFiles(PluginManager.PluginDirectory, "*" + DbFileName, SearchOption.AllDirectories).FirstOrDefault();
			}

			Instance = new SqlData(DataFilePath);
		}

		internal SqlData(string path)
			: base(path)
		{
			masterpieceSupplyDutyCache = Table<MasterpieceSupplyDutyResult>().ToDictionary(key => key.Id, val => val);
			requiredItemCache = Table<RequiredItemResult>().ToDictionary(key => key.Id, val => val);
		}

		public uint? GetIndexByEngName(string engName)
		{
			////			var result = Query<MasterpieceSupplyDutyResult>(@"
			////select m.*
			////from MasterpieceSupplyDutyResult m
			////join RequiredItemResult r on m.Id = r.MasterpieceSupplyDutyResultId
			////where r.EngName = ?", engName).SingleOrDefault();

			////			return result?.Index;

			var requiredItem =
				requiredItemCache.FirstOrDefault(
					kvp => string.Equals(kvp.Value.EngName, engName, StringComparison.OrdinalIgnoreCase)).Value;

			if (requiredItem == null)
			{
				return null;
			}

			MasterpieceSupplyDutyResult masterpieceSupplyDuty;
			if (!masterpieceSupplyDutyCache.TryGetValue(requiredItem.MasterpieceSupplyDutyResultId, out masterpieceSupplyDuty))
			{
				return null;
			}

			return masterpieceSupplyDuty.Index;
		}

		public uint? GetIndexByName(string name)
		{
			var requiredItem =
				requiredItemCache.FirstOrDefault(
					kvp => string.Equals(kvp.Value.CurrentLocaleName, name, StringComparison.OrdinalIgnoreCase)).Value;

			if (requiredItem == null)
			{
				return null;
			}

			MasterpieceSupplyDutyResult masterpieceSupplyDuty;
			if (!masterpieceSupplyDutyCache.TryGetValue(requiredItem.MasterpieceSupplyDutyResultId, out masterpieceSupplyDuty))
			{
				return null;
			}

			return masterpieceSupplyDuty.Index;
		}

		public uint? GetIndexByItemId(uint itemId)
		{
			////			var result = Query<MasterpieceSupplyDutyResult>(@"
			////select m.*
			////from MasterpieceSupplyDutyResult m
			////join RequiredItemResult r on m.Id = r.MasterpieceSupplyDutyResultId
			////where r.Id = ?", itemId).SingleOrDefault();

			////			return result?.Index;

			RequiredItemResult requiredItem;
			if (!requiredItemCache.TryGetValue(itemId, out requiredItem))
			{
				return null;
			}

			MasterpieceSupplyDutyResult masterpieceSupplyDuty;
			if (!masterpieceSupplyDutyCache.TryGetValue(requiredItem.MasterpieceSupplyDutyResultId, out masterpieceSupplyDuty))
			{
				return null;
			}

			return masterpieceSupplyDuty.Index;
		}
	}
}