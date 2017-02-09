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
    using Buddy.Coroutines;
    using System.Threading;
    using ff14bot.RemoteWindows;
    using ff14bot.Behavior;
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

        [XmlAttribute("SpellDelay")]
        public int SpellDelay { get; set; }

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
            return new ExCoroutineAction(ctx => TheMain(), this);
		}

        protected async Task<bool> TheMain()
        {
            bool flag = await Main();

            if (flag)
            {
                await DoMainSuccess();
            } else
            {
                await DoMainFailed();
            }
            return flag;
        }

        protected virtual async Task<bool> DoMainSuccess() {
            return true;
        }

        protected virtual async Task<bool> DoMainFailed() {
            return true;
        }

		protected virtual void DoReset() {}

		protected abstract Task<bool> Main();

		protected override sealed void OnResetCachedDone()
		{
			DoReset();
			isDone = false;
		}

        #region Ability Checks and Actions

        internal async Task<bool> CastAura(uint spellId, int auraId = -1)
        {
            await Coroutine.Yield();
            bool flag =  await Actions.CastAura(spellId, SpellDelay, auraId);
            await Coroutine.Sleep(250);
            return flag;
        }

        internal async Task<bool> CastAura(Ability ability, AbilityAura auraId = AbilityAura.None)
        {
            await Coroutine.Yield();
            bool flag = await Actions.CastAura(ability, SpellDelay, auraId);
            await Coroutine.Sleep(250);
            return flag;
        }

        internal bool HasAura(AbilityAura auraId)
        {
            return Me.HasAura((int)auraId);
        }
        
        internal virtual async Task<bool> Cast(uint id)
        {
            await Coroutine.Yield();
            bool flag = await Actions.Cast(id, SpellDelay);
            await Coroutine.Sleep(250);
            return flag;
        }

        internal virtual async Task<bool> Cast(Ability id)
        {
            await Coroutine.Yield();
            bool flag = await Actions.Cast(id, SpellDelay);
            await Coroutine.Sleep(250);
            return flag;
        }
        
        #endregion

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