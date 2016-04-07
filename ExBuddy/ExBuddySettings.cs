namespace ExBuddy
{
	using System.ComponentModel;
	using System.IO;
	using ff14bot.Helpers;

	public class ExBuddySettings : JsonSettings
	{
		private static ExBuddySettings instance;

		public ExBuddySettings()
			: base(Path.Combine(JsonSettings.SettingsPath, "ExBuddySettings.json")) {}

		[DefaultValue(true)]
		public bool CharacterNameInWindowTitle { get; set; }

		public static ExBuddySettings Instance
		{
			get { return instance ?? (instance = new ExBuddySettings()); }
		}

		[DefaultValue(true)]
		public bool VerboseLogging { get; set; }
	}
}