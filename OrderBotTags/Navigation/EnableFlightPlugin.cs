namespace ExBuddy.OrderBotTags.Navigation
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using ExBuddy.OrderBotTags.Common;

    using ff14bot;
    using ff14bot.AClasses;
    using ff14bot.Behavior;
    using ff14bot.Helpers;
    using ff14bot.Navigation;

    using TreeSharp;

    public class EnableFlightPlugin : BotPlugin
    {
        private Composite startCoroutine;

        private FlightEnabledNavigator navigator;

        private BotEvent cleanup;

        public override string Author
        {
            get { return "ExMatt"; }
        }

        public override string Name
        {
            get { return "EnableFlight"; }
        }

        public override System.Version Version
        {
            get
            {
                return new Version(0, 9, 0);
            }
        }

        private async Task<bool> Start()
        {
            if (navigator == null)
            {
                navigator = new FlightEnabledNavigator(
                    Navigator.NavigationProvider,
                    new FlightEnabledSlideMover(Navigator.PlayerMover),
                    new FlightNavigationArgs
                    {
                        ForcedAltitude = EnableFlightSettings.Instance.ForcedAltitude,
                        InverseParabolicMagnitude = EnableFlightSettings.Instance.InverseParabolicMagnitude,
                        LogWaypoints = EnableFlightSettings.Instance.LogWaypoints,
                        Radius = EnableFlightSettings.Instance.Radius,
                        Smoothing = EnableFlightSettings.Instance.Smoothing
                    });

                cleanup = bot =>
                {
                    DoCleanup();
                    DisposeNav();
                    TreeRoot.OnStop -= cleanup;
                };

                TreeRoot.OnStop += cleanup;

                Logging.Write(Colors.DeepSkyBlue, "Started Flight Navigator.");
            }

            return false;
        }

        private void DoCleanup()
        {
            if (navigator != null)
            {
                Logging.Write(Colors.DeepSkyBlue, "Stopped Flight Navigator.");
                navigator.Dispose();
                navigator = null;
            }
        }

        private void DisposeNav()
        {
            var nav = Navigator.NavigationProvider as GaiaNavigator;
            if (nav != null)
            {
                Logging.Write(Colors.DeepSkyBlue, "Disposing the GaiaNavigator");
                nav.Dispose();
                Navigator.NavigationProvider = null;
            }
        }

        public override void OnShutdown()
        {
            TreeRoot.OnStop -= cleanup;
            DoCleanup();
        }

        public override void OnInitialize()
        {
            startCoroutine = new ActionRunCoroutine(ctx => Start());
        }

        public override void OnDisabled()
        {
            TreeHooks.Instance.OnHooksCleared -= OnHooksCleared;
            TreeHooks.Instance.RemoveHook("TreeStart", startCoroutine);
            TreeRoot.OnStop -= cleanup;
            DoCleanup();
        }

        public override void OnEnabled()
        {
            TreeHooks.Instance.AddHook("TreeStart", startCoroutine);
            TreeHooks.Instance.OnHooksCleared += OnHooksCleared;
        }

        private void OnHooksCleared(object sender, EventArgs args)
        {
            TreeHooks.Instance.AddHook("TreeStart", startCoroutine);
        }
    }

    public class EnableFlightSettings : JsonSettings
    {
        private static EnableFlightSettings instance;

        public static EnableFlightSettings Instance
        {
            get
            {
                return instance ?? (instance = new EnableFlightSettings("EnableFlightSettings"));
            }
        }

        public EnableFlightSettings(string path)
            : base(Path.Combine(CharacterSettingsDirectory, "EnableFlight.json"))
        {
            
        }

        [DefaultValue(2.7f)]
        public float Radius { get; set; }

        [DefaultValue(10)]
        public int InverseParabolicMagnitude { get; set; }

        [DefaultValue(0.2f)]
        public float Smoothing { get; set; }

        [DefaultValue(8.0f)]
        public float ForcedAltitude { get; set; }

        [DefaultValue(false)]
        public bool LogWaypoints { get; set; }
    }
}
