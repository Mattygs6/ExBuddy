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
			get { return isDone; }
		}

		[XmlAttribute("Name")]
		public string Name { get; set; }

		public override sealed string StatusText
		{
			get { return string.Concat(GetType().Name, ": ", statusText); }

			set { statusText = value; }
		}

		protected internal static LocalPlayer Me
		{
			get { return GameObjectManager.LocalPlayer; }
		}

		protected virtual Color Error
		{
			get { return Logger.Colors.Error; }
		}

		protected virtual Color Info
		{
			get { return Logger.Colors.Info; }
		}

		protected virtual Color Warn
		{
			get { return Logger.Colors.Warn; }
		}

		public override string ToString()
		{
			return this.DynamicToString("StatusText", "Behavior");
		}

		protected override Composite CreateBehavior()
		{
			return new ExCoroutineAction(ctx => Main(), this);
		}

		protected virtual void DoReset() {}

		protected abstract Task<bool> Main();

		protected override sealed void OnResetCachedDone()
		{
			DoReset();
			isDone = false;
		}

		#region ILogColors Members

		Color ILogColors.Error
		{
			get { return Error; }
		}

		Color ILogColors.Info
		{
			get { return Info; }
		}

		Color ILogColors.Warn
		{
			get { return Warn; }
		}

		#endregion
	}
}