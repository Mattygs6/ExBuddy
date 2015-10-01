namespace ExBuddy.OrderBotTags.Behaviors
{
	using System.Threading.Tasks;
	using System.Windows.Media;

	using Clio.XmlEngine;

	using ExBuddy.Attributes;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ExBuddy.Logging;

	using ff14bot.Managers;
	using ff14bot.NeoProfiles;
	using ff14bot.Objects;

	using TreeSharp;

	public abstract class ExProfileBehavior : ProfileBehavior, ILogColors
	{
		protected internal readonly Logger Logger;

		// ReSharper disable once InconsistentNaming
		protected bool isDone;

		private string statusText;

		static ExProfileBehavior()
		{
			ReflectionHelper.CustomAttributes<LoggerNameAttribute>.RegisterByAssembly();

			// Until we find a better way to do it.
			Condition.AddNamespacesToScriptManager("ExBuddy", "ExBuddy.Helpers");
		}

		protected ExProfileBehavior()
		{
			Logger = new Logger(this, includeVersion: true);
		}

		public override sealed bool IsDone
		{
			get
			{
				return isDone;
			}
		}

		[XmlAttribute("Name")]
		public string Name { get; set; }

		public override sealed string StatusText
		{
			get
			{
				return string.Concat(this.GetType().Name, ": ", statusText);
			}

			set
			{
				statusText = value;
			}
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
				return Logger.Colors.Error;
			}
		}

		protected virtual Color Info
		{
			get
			{
				return Logger.Colors.Info;
			}
		}

		protected virtual Color Warn
		{
			get
			{
				return Logger.Colors.Warn;
			}
		}

		#region ILogColors Members

		Color ILogColors.Error
		{
			get
			{
				return this.Error;
			}
		}

		Color ILogColors.Info
		{
			get
			{
				return this.Info;
			}
		}

		Color ILogColors.Warn
		{
			get
			{
				return this.Warn;
			}
		}

		#endregion

		public override string ToString()
		{
			return this.DynamicToString("StatusText", "Behavior");
		}

		protected override Composite CreateBehavior()
		{
			return new ActionRunCoroutine(ctx => Main());
		}

		protected virtual void DoReset() {}

		protected abstract Task<bool> Main();

		protected override sealed void OnResetCachedDone()
		{
			DoReset();
			isDone = false;
		}
	}
}