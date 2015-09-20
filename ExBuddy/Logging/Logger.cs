namespace ExBuddy.Logging
{
    using System;
    using System.Globalization;
    using System.Reflection;

    using Clio.XmlEngine;

    using ExBuddy.Interfaces;
    using Logging = ff14bot.Helpers.Logging;

    public class Logger
    {
        private static readonly Version Version;
        private readonly string name;
        private readonly ILogColors logColors;

        static Logger()
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (assembly.IsDefined(typeof(AssemblyFileVersionAttribute)))
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
            : this(new LogColors())
        {
        }

        public Logger(ILogColors logColors)
        {
            var type = logColors.GetType();
            this.name = type.GetCustomAttributePropertyValue<XmlElementAttribute, string>(attr => attr.Name, type.Name);
            this.logColors = logColors;
        }

        private string Prefix
        {
            get
            {
                return string.Format("[{0} v{1}] ", name, Version);
            }
        }

        public void Info(string message)
        {
            Logging.Write(logColors.Info, Prefix + message);
        }

        public void Info(string format, params object[] args)
        {
            Logging.Write(logColors.Info, Prefix + string.Format(CultureInfo.InvariantCulture, format, args));
        }

        public void Warn(string message)
        {
            Logging.Write(logColors.Warn, Prefix + message);
        }

        public void Warn(string format, params object[] args)
        {
            Logging.Write(logColors.Warn, Prefix + string.Format(CultureInfo.InvariantCulture, format, args));
        }

        public void Error(string message)
        {
            Logging.Write(logColors.Error, Prefix + message);
        }

        public void Error(string format, params object[] args)
        {
            Logging.Write(logColors.Error, Prefix + string.Format(CultureInfo.InvariantCulture, format, args));
        }
    }
}
