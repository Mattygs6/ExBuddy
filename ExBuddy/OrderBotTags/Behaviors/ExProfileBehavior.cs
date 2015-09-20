namespace ExBuddy.OrderBotTags.Behaviors
{
    using System.Linq;
    using System.Reflection;
    using System.Windows.Media;

    using Clio.XmlEngine;

    using ExBuddy.Helpers;
    using ExBuddy.Interfaces;
    using ExBuddy.Logging;

    using ff14bot.Managers;
    using ff14bot.NeoProfiles;
    using ff14bot.Objects;

    public abstract class ExProfileBehavior : ProfileBehavior, ILogColors
    {
        private static readonly LogColors Colors = new LogColors();
        protected internal readonly Logger Logger;

        static ExProfileBehavior()
        {
            var types =
                Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => !t.IsAbstract && t.GetCustomAttribute<XmlElementAttribute>(false) != null)
                    .ToArray();

            ReflectionHelper.CustomAttributes<XmlElementAttribute>.RegisterTypes(types);
        }

        protected ExProfileBehavior()
        {
            Logger = new Logger(this);
        }

        protected internal static LocalPlayer Me
        {
            get
            {
                return GameObjectManager.LocalPlayer;
            }
        }

        protected virtual Color Error
        {
            get
            {
                return Colors.Error;
            }
        }

        protected virtual Color Warn
        {
            get
            {
                return Colors.Warn;
            }
        }

        protected virtual Color Info
        {
            get
            {
                return Colors.Info;
            }
        }


        Color ILogColors.Error
        {
            get
            {
                return this.Error;
            }
        }

        Color ILogColors.Warn
        {
            get
            {
                return this.Warn;
            }
        }

        Color ILogColors.Info
        {
            get
            {
                return this.Info;
            }
        }
    }
}
