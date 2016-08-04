namespace ExBuddy.Windows
{
	using System;
	using System.Linq;
	using System.Reflection;
	using ExBuddy.Enumerations;
	using ExBuddy.Offsets;
	using ff14bot;
	using ff14bot.Managers;

	public sealed class GuildLeve : Window<GuildLeve>
	{
		private static readonly Type LeveManagerType =
			Assembly.GetEntryAssembly()
				.GetTypes()
				.FirstOrDefault(
					t =>
						t.GetProperties(BindingFlags.Static | BindingFlags.Public).Count(f => f.PropertyType == typeof (LeveWork[])) == 1);

		private static readonly PropertyInfo LevesPropertyInfo =
			LeveManagerType.GetProperties(BindingFlags.Static | BindingFlags.Public)
				.FirstOrDefault(f => f.PropertyType == typeof (LeveWork[]));

		public GuildLeve()
			: base("GuildLeve") {}

		public static LeveWork[] ActiveLeves
		{
			get { return LevesPropertyInfo.GetValue(null) as LeveWork[]; }
		}

		public static int Allowances
		{
			get { return Core.Memory.NoCacheRead<int>(GuildLeveOffsets.AllowancesPtr); }
		}

		public SendActionResult AcceptLeve(uint guildLeveId)
		{
			return TrySendAction(2, 3, 2, 4, guildLeveId);
		}

		public static bool HasLeve(uint leveId)
		{
			var activeLeves = GuildLeve.ActiveLeves;

			return activeLeves.Any(leve => leve.GlobalId == leveId);
		}

		public static bool HasLeves(uint[] leveIds)
		{
			if (leveIds == null)
			{
				return false;
			}

			var activeLeves = GuildLeve.ActiveLeves;

			return leveIds.All(leveId => activeLeves.Any(leve => leve.GlobalId == leveId));
		}
	}
}