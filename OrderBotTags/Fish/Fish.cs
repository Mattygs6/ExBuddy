namespace ExBuddy.OrderBotTags
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Windows.Media;

    using Clio.Common;
    using Clio.Utilities;
    using Clio.XmlEngine;

    using ff14bot;
    using ff14bot.Behavior;
    using ff14bot.Enums;
    using ff14bot.Helpers;
    using ff14bot.Managers;
    using ff14bot.NeoProfiles;
    using ff14bot.RemoteWindows;
    using ff14bot.Settings;

    using TreeSharp;

    using Action = TreeSharp.Action;

    [XmlElement("Fish")]
    public class FishTag : ProfileBehavior
    {
        [Serializable]
        public enum Abilities
        {
            None = -1,

            Sprint = 3,

            Bait = 288,

            Cast = 289,

            Hook = 296,

            Mooch = 297,

            Stealth = 298,

            Quit = 299,

            Release = 300,

            CastLight = 2135,

            Snagging = 4100,

            CollectorsGlove = 4101,

            Patience = 4102,

            PowerfulHookset = 4103,

            Chum = 4104,

            FishEyes = 4105,

            PrecisionHookset = 4179,

            Patience2 = 4106
        }

        private const uint WM_KEYDOWN = 0x100;

        private const uint WM_KEYUP = 0x0101;

        protected bool HasPatience
        {
            get
            {
                // Gathering Fortune Up (Fishing)
                return Core.Player.HasAura(850);
            }
        }

        protected bool HasSnagging
        {
            get
            {
                // Snagging
                return Core.Player.HasAura(761);
            }
        }

        protected bool HasCollectorsGlove
        {
            get
            {
                // Collector's Glove
                return Core.Player.HasAura(805);
            }
        }

        protected bool HasChum
        {
            get
            {
                // Chum
                return Core.Player.HasAura(763);
            }
        }

        protected bool HasFishEyes
        {
            get
            {
                // Fish Eyes
                return Core.Player.HasAura(762);
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        protected static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        protected static void PostKeyPress(VirtualKeys key)
        {
            PostKeyPress((int)key);
        }

        protected static void PostKeyPress(int key)
        {
            PostMessage(Core.Memory.Process.MainWindowHandle, WM_KEYDOWN, new IntPtr(key), IntPtr.Zero);
            PostMessage(Core.Memory.Process.MainWindowHandle, WM_KEYUP, new IntPtr(key), IntPtr.Zero);
        }

        public static bool IsFishing()
        {
            return isFishing;
        }

        protected override void OnStart()
        {
            if (this.Keepers == null)
            {
                this.Keepers = new List<Keeper>();
            }

            if (this.Collectables == null)
            {
                this.Collectables = new List<Collectable>();
            }

            GamelogManager.MessageRecevied += ReceiveMessage;
            FishSpots.IsCyclic = true;
            isFishing = false;
            isSitting = false;
            initialMountSetting = CharacterSettings.Instance.UseMount;
            ShuffleFishSpots();

            sitRoll = SitRng.NextDouble();

            if (IsBaitWindowOpen && CanDoAbility(Abilities.Bait))
            {
                DoAbility(Abilities.Bait);
            }

            if (CanDoAbility(Abilities.Quit))
            {
                DoAbility(Abilities.Quit);
            }

            cleanup = bot =>
                {
                    this.DoCleanup();
                    TreeRoot.OnStop -= cleanup;
                };

            TreeRoot.OnStop += cleanup;
        }

        protected override void OnDone()
        {

            TreeRoot.OnStop -= cleanup;
            this.DoCleanup();
        }

        protected virtual void DoCleanup()
        {
            try
            {
                GamelogManager.MessageRecevied -= this.ReceiveMessage;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }

            isFishing = false;
            CharacterSettings.Instance.UseMount = initialMountSetting;
        }

        protected override void OnResetCachedDone()
        {
            baitAttempts = 0;
            baitChanged = false;
            baitCount = GetBaitCount();
            isDone = false;
            mooch = 0;
            sitRoll = 1.0;
            spotinit = false;
            fishcount = 0;
            amissfish = 0;
            isFishing = false;
            isSitting = false;
            isFishIdentified = false;
            fishlimit = GetFishLimit();
            currentBait = string.Empty;
            checkRelease = false;

            CharacterSettings.Instance.UseMount = initialMountSetting;
        }

        protected override Composite CreateBehavior()
        {
            fishlimit = GetFishLimit();
            baitCount = GetBaitCount();
            baitAttempts = 0;

            return new PrioritySelector(
                StateTransitionAlwaysSucceed,
                Conditional,
                Blacklist,
                MoveToFishSpot,
                GoFish(
                    StopMovingComposite,
                    DismountComposite,
                    CheckStealthComposite,
                    CheckWeatherComposite,
                // Waits up to 10 hours, might want to rethink this one.
                    EnsureBaitComposite,
                    OpenBaitComposite,
                    ApplyBaitComposite,
                    CloseBaitComposite,
                    InitFishSpotComposite,
                    CollectablesComposite,
                    ReleaseComposite,
                    MoochComposite,
                    FishCountLimitComposite,
                    InventoryFullComposite,
                    SitComposite,
                    CollectorsGloveComposite,
                    SnaggingComposite,
                    PatienceComposite,
                    FishEyesComposite,
                    ChumComposite,
                    CastComposite,
                    HookComposite));
        }

        protected Composite GoFish(params Composite[] children)
        {
            return
                new PrioritySelector(
                    new Decorator(
                        ret => Vector3.Distance(Core.Player.Location, FishSpots.CurrentOrDefault.XYZ) < 2,
                        new PrioritySelector(children)));
        }

        #region Fields

        private static bool isFishing;

        protected static readonly Random SitRng = new Random();

        protected static Regex FishRegex = new Regex(
            @"You land an{0,1} (.+) measuring (\d{1,4}\.\d) ilms!",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected static Regex BaitRegex = new Regex(
            @"You apply an{0,1} (.+) to your line.",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected static FishResult FishResult = new FishResult();

        private bool initialMountSetting;

        private BotEvent cleanup;

        private string currentBait;

        private int baitCount;

        private bool baitChanged;

        private int baitAttempts;

        private bool checkRelease;

        private bool isSitting;

        private bool isDone;

        private bool isFishIdentified;

        private int mooch;

        private int fishcount;

        private int amissfish;

        private int fishlimit;

        private double sitRoll = 1.0;

        private bool spotinit;

        #endregion

        #region Public Properties

        [XmlElement("Keepers")]
        public List<Keeper> Keepers { get; set; }

        [XmlElement("Collectables")]
        public List<Collectable> Collectables { get; set; }

        [XmlElement("FishSpots")]
        public IndexedList<FishSpot> FishSpots { get; set; }

        [DefaultValue(0)]
        [XmlAttribute("Mooch")]
        public int MoochLevel { get; set; }

        [DefaultValue("True")]
        [XmlAttribute("MoochCondition")]
        public string MoochCondition { get; set; }

        [DefaultValue(20)]
        [XmlAttribute("MinFish")]
        public int MinimumFishPerSpot { get; set; }

        [DefaultValue(30)]
        [XmlAttribute("MaxFish")]
        public int MaximumFishPerSpot { get; set; }

        [XmlAttribute("Bait")]
        public string Bait { get; set; }

        [DefaultValue(1200)]
        [XmlAttribute("BaitDelay")]
        public int BaitDelay { get; set; }

        [XmlAttribute("Chum")]
        public bool Chum { get; set; }

        [DefaultValue(VirtualKeys.Numpad0)]
        [XmlAttribute("ConfirmKey")]
        public VirtualKeys ConfirmKey { get; set; }

        [DefaultValue(30)]
        [XmlAttribute("LastFishTimeout")]
        public int LastFishTimeout { get; set; }

        [DefaultValue(VirtualKeys.Numpad6)]
        [XmlAttribute("MoveCursorRightKey")]
        public VirtualKeys MoveCursorRightKey { get; set; }

        [DefaultValue("True")]
        [XmlAttribute("Condition")]
        public string Condition { get; set; }

        [XmlAttribute("Weather")]
        public string Weather { get; set; }

        [XmlAttribute("ShuffleFishSpots")]
        public bool Shuffle { get; set; }

        [XmlAttribute("SitRate")]
        public float SitRate { get; set; }

        [XmlAttribute("Sit")]
        public bool Sit { get; set; }

        [XmlAttribute("Stealth")]
        public bool Stealth { get; set; }

        [XmlAttribute("Collect")]
        public bool Collect { get; set; }

        [XmlAttribute("CollectabilityValue")]
        public int UCollectabilityValue { get; set; }

        public uint CollectabilityValue
        {
            get
            {
                return Convert.ToUInt32(UCollectabilityValue);
            }
        }

        [DefaultValue(Abilities.None)]
        [XmlAttribute("Patience")]
        public Abilities Patience { get; set; }

        [XmlAttribute("FishEyes")]
        public bool FishEyes { get; set; }

        [XmlAttribute("Snagging")]
        public bool Snagging { get; set; }

        [DefaultValue(Abilities.PowerfulHookset)]
        [XmlAttribute("Hookset")]
        public Abilities Hookset { get; set; }

        public override bool IsDone
        {
            get
            {
                return this.isDone;
            }
        }

        public Version Version
        {
            get
            {
                return new Version(3, 0, 7, 201508254);
            }
        }

        #endregion

        #region Private Properties

        private bool HasSpecifiedBait
        {
            get
            {
                return
                    InventoryManager.FilledSlots.Any(
                        i => string.Equals(i.Name, this.Bait, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        private bool IsBaitWindowOpen
        {
            get
            {
                return RaptureAtkUnitManager.Controls.Any(c => c.Name == "Bait" && c.IsValid);
            }
        }

        private bool IsBaitSpecified
        {
            get
            {
                return !string.IsNullOrEmpty(this.Bait);
            }
        }

        private bool IsCorrectBaitSelected
        {
            get
            {
                return string.Equals(currentBait, this.Bait, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        #endregion

        #region Fishing Composites

        protected Composite DismountComposite
        {
            get
            {
                return new Decorator(ret => Core.Player.IsMounted, CommonBehaviors.Dismount());
            }
        }

        protected Composite CollectablesComposite
        {
            get
            {
                return new Decorator(
                    ret => Collect && SelectYesNoItem.IsOpen,
                    new Wait(
                        10,
                        ret => SelectYesNoItem.CollectabilityValue > Math.Max(20, CollectabilityValue / 6),
                        new Sequence(
                            new Action(
                                r =>
                                {
                                    var value = SelectYesNoItem.CollectabilityValue;
                                    Log(
                                        string.Format(
                                            "Collectible caught with value: {0} required: {1}",
                                            value.ToString(),
                                            CollectabilityValue));

                                    if (value >= CollectabilityValue)
                                    {
                                        Log("Collecting Collectible -> Value: " + value, Colors.Green);
                                        SelectYesNoItem.Yes();
                                    }
                                    else
                                    {
                                        Log("Declining Collectible -> Value: " + value, Colors.Red);
                                        SelectYesNoItem.No();
                                    }
                                }),
                            new Sleep(1111))));
            }
        }

        protected Composite FishCountLimitComposite
        {
            get
            {
                return
                    new Decorator(
                        ret =>
                        fishcount >= fishlimit && !HasPatience && CanDoAbility(Abilities.Quit)
                        && FishingManager.State == FishingState.PoleReady && !SelectYesNoItem.IsOpen,
                        new Sequence(
                            new Sleep(2, 3),
                            new Action(r => { DoAbility(Abilities.Quit); }),
                            new Sleep(2, 3),
                            new Action(r => { ChangeFishSpot(); })));
            }
        }

        protected Composite SitComposite
        {
            get
            {
                return
                    new Decorator(
                        ret =>
                        !isSitting && (Sit || FishSpots.CurrentOrDefault.Sit || sitRoll < SitRate) && FishingManager.State == (FishingState)9,
                    // this is when you have already cast and are waiting for a bite.
                        new Sequence(
                            new Sleep(1, 1),
                            new Action(
                                r =>
                                {
                                    isSitting = true;
                                    Log("Sitting " + FishSpots.CurrentOrDefault);
                                    ChatManager.SendChat("/sit");
                                })));
            }
        }

        protected Composite StopMovingComposite
        {
            get
            {
                return new Decorator(ret => MovementManager.IsMoving, CommonBehaviors.MoveStop());
            }
        }

        protected Composite InitFishSpotComposite
        {
            get
            {
                return new Decorator(
                    ret => !spotinit,
                    new Action(
                        r =>
                        {
                            FaceFishSpot();
                            isFishing = true;
                            Log("Will fish for " + fishlimit + " fish before moving again.");
                            spotinit = true;
                        }));
            }
        }

        protected Composite CheckWeatherComposite
        {
            get
            {
                return new Decorator(
                    ret => Weather != null && Weather != WorldManager.CurrentWeather,
                    new Sequence(
                        new Action(r => { Log("Waiting for the proper weather..."); }),
                        new Wait(36000, ret => Weather == WorldManager.CurrentWeather, new ActionAlwaysSucceed())));
            }
        }

        protected Composite CollectorsGloveComposite
        {
            get
            {
                return new Decorator(
                    ret => CanDoAbility(Abilities.CollectorsGlove) && Collect ^ HasCollectorsGlove,
                    new Sequence(
                        new Action(
                            r =>
                            {
                                Log("Casting Collector's Glove");
                                DoAbility(Abilities.CollectorsGlove);
                            }),
                        new Sleep(2, 3)));
            }
        }

        protected Composite SnaggingComposite
        {
            get
            {
                return new Decorator(
                    ret => CanDoAbility(Abilities.Snagging) && Snagging ^ HasSnagging,
                    new Sequence(
                        new Action(
                            r =>
                            {
                                Log("Toggle Snagging");
                                DoAbility(Abilities.Snagging);
                            }),
                        new Sleep(2, 3)));
            }
        }

        protected Composite MoochComposite
        {
            get
            {
                return
                    new Decorator(
                        ret =>
                        CanDoAbility(Abilities.Mooch) && MoochLevel != 0 && mooch < MoochLevel && MoochConditionCheck() &&
                        (this.Keepers.Count == 0 || this.Keepers.All(k => !string.Equals(k.Name, FishResult.FishName, StringComparison.InvariantCultureIgnoreCase)) || this.Keepers.Any(k => string.Equals(k.Name, FishResult.FishName, StringComparison.InvariantCultureIgnoreCase) && FishResult.ShouldMooch(k))),
                        new Sequence(
                            new Action(
                                r =>
                                {
                                    this.checkRelease = true;
                                    FishingManager.Mooch();
                                    mooch++;
                                    if (MoochLevel > 1)
                                    {
                                        Log("Mooching, this is mooch " + mooch + " of " + MoochLevel + " mooches.");
                                    }
                                    else
                                    {
                                        Log("Mooching, this will be the only mooch.");
                                    }
                                }),
                            new Sleep(2, 2)));
            }
        }

        protected Composite ChumComposite
        {
            get
            {
                return new Decorator(
                    ret => Chum && !HasChum && CanDoAbility(Abilities.Chum),
                    new Sequence(new Action(r => DoAbility(Abilities.Chum)), new Sleep(1, 2)));
            }
        }

        protected Composite PatienceComposite
        {
            get
            {
                return
                    new Decorator(
                        ret =>
                        Patience > Abilities.None
                        && (FishingManager.State == FishingState.None || FishingManager.State == FishingState.PoleReady)
                        && !HasPatience && CanDoAbility(Patience)
                        && (Core.Player.CurrentGP >= 600 || Core.Player.CurrentGPPercent == 100.0f),
                        new Sequence(
                            new Action(
                                r =>
                                {
                                    DoAbility(Patience);
                                    Log("Patience activated");
                                }),
                            new Sleep(1, 2)));
            }
        }

        protected Composite FishEyesComposite
        {
            get
            {
                return new Decorator(
                    ret => FishEyes && !HasFishEyes && CanDoAbility(Abilities.FishEyes),
                    new Sequence(new Action(r => DoAbility(Abilities.FishEyes)), new Sleep(1, 2)));
            }
        }

        protected Composite ReleaseComposite
        {
            get
            {
                return
                    new Decorator(
                        ret =>
                        this.checkRelease && FishingManager.State == FishingState.PoleReady
                        && CanDoAbility(Abilities.Release) && this.Keepers.Count != 0,
                        new Sequence(
                            new Wait(
                                2,
                                ret => this.isFishIdentified,
                                new Action(
                                    r =>
                                    {
                                        // If its not a keeper AND we aren't mooching or we can't mooch, then release
                                        if (!this.Keepers.Any(FishResult.IsKeeper)
                                            && (MoochLevel == 0 || !CanDoAbility(Abilities.Mooch)))
                                        {
                                            DoAbility(Abilities.Release);
                                            Log("Released " + FishResult.Name);
                                        }

                                        this.checkRelease = false;
                                    })),
                            new Wait(2, ret => !CanDoAbility(Abilities.Release), new ActionAlwaysSucceed())));
            }
        }

        protected Composite CastComposite
        {
            get
            {
                return
                    new Decorator(
                        ret =>
                        FishingManager.State == FishingState.None || FishingManager.State == FishingState.PoleReady,
                        new Action(r => this.Cast()));
            }
        }

        protected Composite InventoryFullComposite
        {
            get
            {
                return
                    new Decorator(
                        ret => InventoryManager.FilledSlots.Count(c => c.BagId != InventoryBagId.KeyItems) >= 100,
                        IsDoneAction);
            }
        }

        protected Composite HookComposite
        {
            get
            {
                return new Decorator(
                    ret => FishingManager.CanHook && FishingManager.State == FishingState.Bite,
                    new Action(
                        r =>
                        {
                            if (HasPatience && CanDoAbility(Hookset))
                            {
                                DoAbility(Hookset);
                                Log("Using (" + Hookset + ")");
                            }
                            else
                            {
                                FishingManager.Hook();
                            }

                            amissfish = 0;
                            if (mooch == 0)
                            {
                                fishcount++;
                            }

                            Log("Fished " + fishcount + " of " + fishlimit + " fish at this FishSpot.");
                        }));
            }
        }

        protected Composite CheckStealthComposite
        {
            get
            {
                return new Decorator(
                    ret => this.Stealth && !Core.Player.HasAura(47),
                    new Sequence(
                        new Action(
                            r =>
                            {
                                CharacterSettings.Instance.UseMount = false;
                                DoAbility(Abilities.Stealth);
                            }),
                        new Sleep(2, 3)));
            }
        }

        protected Composite EnsureBaitComposite
        {
            get
            {
                return new Decorator(
                    ret => IsBaitSpecified && !HasSpecifiedBait,
                    new Sequence(
                        new Action(r => { Log("You do not have the specified bait: " + this.Bait); }),
                        IsDoneAction));
            }
        }

        protected Composite OpenBaitComposite
        {
            get
            {
                return
                    new Decorator(
                        ret =>
                        IsBaitSpecified && !IsBaitWindowOpen && !IsCorrectBaitSelected && CanDoAbility(Abilities.Bait),
                        new Sequence(
                            new Action(r => { DoAbility(Abilities.Bait); }),
                            new Wait(3, ret => IsBaitWindowOpen, new ActionAlwaysSucceed())));
            }
        }

        protected Composite ApplyBaitComposite
        {
            get
            {
                return new Decorator(
                    ret => IsBaitSpecified && IsBaitWindowOpen && !IsCorrectBaitSelected,
                    new PrioritySelector(
                        new Decorator(
                            ret => baitCount < 0,
                            new Sequence(
                                new Action(
                                    r => { Log("Unable to find specified bait -> " + this.Bait + ", ending profile"); }),
                                IsDoneAction)),
                        new Sequence(
                            new Wait(
                                TimeSpan.FromMilliseconds(Math.Max(100, this.BaitDelay / 10)),
                                new Action(r => PostKeyPress(this.MoveCursorRightKey))),
                            new Wait(
                                TimeSpan.FromMilliseconds(Math.Max(100, this.BaitDelay / 10)),
                                new Action(
                                    r =>
                                    {
                                        PostKeyPress(this.ConfirmKey);
                                        baitCount++;
                                    })),
                            new WaitContinue(
                                TimeSpan.FromMilliseconds(this.BaitDelay),
                                ret => IsCorrectBaitSelected,
                                new Action(r => { Log("Correct Bait Selected -> " + this.Bait); })),
                            new PrioritySelector(
                                new Decorator(ret => baitChanged, new Action(r => { baitChanged = false; })),
                                new Decorator(
                                    ret => baitCount == 1 && HasSpecifiedBait,
                                    new Action(
                                        r =>
                                        {
                                            currentBait = this.Bait;
                                            Log("Correct Bait Selected -> " + this.Bait);
                                        })),
                                new Decorator(
                                    ret => baitAttempts > 2,
                                    new Sequence(
                                        new Action(
                                            r =>
                                            {
                                                Log("Lost focus on the bait window, select bait manually.");
                                                Log("Attempting to re-apply bait in 30 seconds.");

                                                // reset bait count
                                                baitCount = this.GetBaitCount();
                                            }),
                                        new WaitContinue(
                                            30,
                                            ret => IsCorrectBaitSelected,
                                            new Action(r => { Log("Correct Bait Selected -> " + this.Bait); })))),
                                new ActionAlwaysSucceed()))));
            }
        }

        protected Composite CloseBaitComposite
        {
            get
            {
                return
                    new Decorator(
                        ret =>
                        IsBaitSpecified && IsBaitWindowOpen && IsCorrectBaitSelected && CanDoAbility(Abilities.Bait),
                        new Sequence(
                            new Action(r => { DoAbility(Abilities.Bait); }),
                            new Wait(5, ret => !IsBaitWindowOpen, new ActionAlwaysSucceed())));
            }
        }

        #endregion

        #region Composites

        protected Composite Conditional
        {
            get
            {
                return new Decorator(ret => FishingManager.State < FishingState.Bite && !ConditionCheck(), IsDoneAction);
            }
        }

        protected Composite Blacklist
        {
            get
            {
                return new Decorator(
                    ret => amissfish > Math.Min(FishSpots.Count, 4),
                    new Sequence(
                        new Action(
                            r =>
                            {
                                Log("The fish are amiss at all of the FishSpots.");
                                Log(
                                    "This zone has been blacklisted, please fish somewhere else and then restart the profile.");
                            }),
                        IsDoneAction));
            }
        }

        protected Composite StateTransitionAlwaysSucceed
        {
            get
            {
                return
                    new Decorator(
                        ret =>
                        FishingManager.State == FishingState.Reelin || FishingManager.State == FishingState.Quit
                        || FishingManager.State == FishingState.PullPoleIn,
                        new ActionAlwaysSucceed());
            }
        }


        protected Composite MoveToFishSpot
        {
            get
            {
                return new Decorator(
                    ret => Vector3.Distance(Core.Player.Location, FishSpots.CurrentOrDefault.XYZ) > 1,
                    CommonBehaviors.MoveAndStop(ret => FishSpots.CurrentOrDefault.XYZ, 1, true));
            }
        }

        protected Composite IsDoneAction
        {
            get
            {
                return
                    new Sequence(
                        new WaitContinue(
                            this.LastFishTimeout,
                            ret => FishingManager.State < FishingState.Bite,
                            new Sequence(
                                new PrioritySelector(CollectablesComposite, ReleaseComposite, new ActionAlwaysSucceed()),
                                new Sleep(2, 3),
                                new Action(r => DoAbility(Abilities.Quit)),
                                new Sleep(2, 3),
                                new Action(r => { this.isDone = true; }))));
            }
        }

        #endregion

        #region Ability Checks and Actions

        protected bool CanDoAbility(Abilities ability)
        {
            return Actionmanager.CanCast((uint)ability, Core.Player);
        }

        protected bool DoAbility(Abilities ability)
        {
            return Actionmanager.DoAction((uint)ability, Core.Player);
        }

        #endregion

        #region Methods

        protected virtual bool ConditionCheck()
        {
            var conditional = ScriptManager.GetCondition(Condition);

            return conditional();
        }

        protected virtual bool MoochConditionCheck()
        {
            var moochConditional = ScriptManager.GetCondition(MoochCondition);

            return moochConditional();
        }

        protected virtual void Cast()
        {
            this.isFishIdentified = false;
            this.checkRelease = true;
            FishingManager.Cast();
            this.ResetMooch();
        }

        protected virtual void FaceFishSpot()
        {
            var i = MathEx.Random(0, 25);
            i = i / 100;

            var i2 = MathEx.Random(0, 100);

            if (i2 > 50)
            {
                Core.Player.SetFacing(FishSpots.Current.Heading - (float)i);
            }
            else
            {
                Core.Player.SetFacing(FishSpots.Current.Heading + (float)i);
            }
        }

        protected virtual void ChangeFishSpot()
        {
            FishSpots.Next();
            Log("Changing FishSpots...");
            fishcount = 0;
            Log("Resetting fish count...");
            fishlimit = this.GetFishLimit();
            sitRoll = SitRng.NextDouble();
            spotinit = false;
            isFishing = false;
            isSitting = false;
        }

        protected virtual int GetBaitCount()
        {
            var result =
                InventoryManager.FilledSlots.Where(
                    bs => bs.Item.Affinity == 19 && bs.Item.RequiredLevel <= Core.Player.ClassLevel)
                    .GroupBy(
                        bs => bs.RawItemId,
                        (key, groupedItems) => new { ItemId = key, Count = groupedItems.Sum(gi => gi.Count) })
                    .ToArray();

            return result.Length;
        }

        protected virtual int GetFishLimit()
        {
            return Convert.ToInt32(MathEx.Random(this.MinimumFishPerSpot, this.MaximumFishPerSpot));
        }

        protected void ShuffleFishSpots()
        {
            if (Shuffle && FishSpots.Index == 0)
            {
                FishSpots.Shuffle();
                Log("Shuffled fish spots");
            }
        }

        protected void ResetMooch()
        {
            if (mooch != 0)
            {
                mooch = 0;
                Log("Resetting mooch level.");
            }
        }

        protected string GetCurrentBait(string message)
        {
            if (BaitRegex.IsMatch(message))
            {
                var match = BaitRegex.Match(message);

                return match.Groups[1].Value;
            }

            return "Parse Error";
        }

        protected void SetFishResult(string message)
        {
            var fishResult = new FishResult();

            var match = FishRegex.Match(message);

            if (match.Success)
            {
                fishResult.Name = match.Groups[1].Value;
                float size;
                float.TryParse(match.Groups[2].Value, out size);
                fishResult.Size = size;

                if (fishResult.Name[fishResult.Name.Length - 2] == ' ')
                {
                    fishResult.IsHighQuality = true;
                }
            }

            FishResult = fishResult;
            this.isFishIdentified = true;
        }

        protected void ReceiveMessage(object sender, ChatEventArgs e)
        {
            if (e.ChatLogEntry.MessageType == MessageType.SystemMessages
                && e.ChatLogEntry.Contents.StartsWith("You apply"))
            {
                currentBait = GetCurrentBait(e.ChatLogEntry.Contents);
                baitCount--;
                baitChanged = true;
                Log("Applied Bait -> " + currentBait);
            }

            if (e.ChatLogEntry.MessageType == (MessageType)2115 && e.ChatLogEntry.Contents.StartsWith("You land"))
            {
                SetFishResult(e.ChatLogEntry.Contents);
            }

            if (e.ChatLogEntry.MessageType == (MessageType)2115
                && e.ChatLogEntry.Contents.Equals(
                    "You do not sense any fish here.",
                    StringComparison.InvariantCultureIgnoreCase))
            {
                Log("You do not sense any fish here, trying next location.");

                if (CanDoAbility(Abilities.Quit))
                {
                    DoAbility(Abilities.Quit);
                }

                ChangeFishSpot();
            }

            if (e.ChatLogEntry.MessageType == (MessageType)2115
                && e.ChatLogEntry.Contents
                == "The fish sense something amiss. Perhaps it is time to try another location.")
            {
                Log("The fish sense something amiss!");
                amissfish++;

                if (CanDoAbility(Abilities.Quit))
                {
                    DoAbility(Abilities.Quit);
                }

                ChangeFishSpot();
            }
        }

        protected void Log(string message, Color color)
        {
            Logging.Write(color, string.Format("[Fish v{0}] {1}", this.Version, message));
        }

        protected void Log(string message)
        {
            Log(message, Colors.Gold);
        }

        #endregion
    }
}