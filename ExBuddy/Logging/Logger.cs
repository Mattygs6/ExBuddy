namespace ExBuddy.Logging
{
	using System;
	using System.Globalization;
	using System.Reflection;
	using Clio.Utilities;
	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;
	using ff14bot.Helpers;

	public sealed class Logger
	{
		public static readonly LogColors Colors = new LogColors();

		public static readonly Logger Instance = new Logger(new LogColors(), "ExBuddy");

		internal static readonly Version Version;

		private readonly ILogColors logColors;

		static Logger()
		{
			var assembly = Assembly.GetExecutingAssembly();
			if (assembly.IsDefined(typeof (AssemblyFileVersionAttribute)))
			{
				try
				{
					var versionAttr = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
					Version = new Version(versionAttr.Version);
					return;
				}
				catch
				{
					// ignored
				}
			}

			// Give a generic version here, won't need to worry about this if i switch to using a dll.
			Version = new Version(3, 0, 7);
		}

		public Logger()
			: this(new LogColors()) {}

		public Logger(ILogColors logColors, string name = null, bool includeVersion = false)
		{
			this.logColors = logColors;
			IncludeVersion = includeVersion;

			if (!string.IsNullOrWhiteSpace(name))
			{
				Name = name;
				return;
			}

			var type = logColors.GetType();
			Name = type.GetCustomAttributePropertyValue<LoggerNameAttribute, string>(attr => attr.Name, type.Name);
		}

		public bool IncludeVersion { get; internal set; }

		public string Name { get; internal set; }

		private string Prefix
		{
			get
			{
				if (IncludeVersion)
				{
					return string.Format("[{0} v{1}] ", Name, Version);
				}

				return string.Format("[{0}] ", Name);
			}
		}

		public void Error(string message)
		{
			Logging.Write(logColors.Error, Prefix + message);
		}

		[StringFormatMethod("format")]
		public void Error(string format, params object[] args)
		{
			Logging.Write(logColors.Error, Prefix + string.Format(CultureInfo.InvariantCulture, format, args));
		}

		public void Info(string message)
		{
			Logging.Write(logColors.Info, Prefix + message);
		}

		[StringFormatMethod("format")]
		public void Info(string format, params object[] args)
		{
			Logging.Write(logColors.Info, Prefix + string.Format(CultureInfo.InvariantCulture, format, args));
		}

		public void Verbose(string message)
		{
			if (ExBuddySettings.Instance.VerboseLogging)
			{
				Logging.WriteVerbose(logColors.Info, Prefix + message);
			}
		}

		[StringFormatMethod("format")]
		public void Verbose(string format, params object[] args)
		{
			if (ExBuddySettings.Instance.VerboseLogging)
			{
				Logging.WriteVerbose(logColors.Info, Prefix + string.Format(CultureInfo.InvariantCulture, format, args));
			}
		}

		public void Warn(string message)
		{
			Logging.Write(logColors.Warn, Prefix + message);
		}

		[StringFormatMethod("format")]
		public void Warn(string format, params object[] args)
		{
			Logging.Write(logColors.Warn, Prefix + string.Format(CultureInfo.InvariantCulture, format, args));
		}
	}
}