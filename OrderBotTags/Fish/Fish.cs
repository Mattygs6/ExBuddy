namespace ExBuddy.OrderBotTags
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Threading;
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

            Patience2 = 9001 // Need to Check this value when i get skill
        }

        private const uint WM_KEYDOWN = 0x100;

        private const uint WM_KEYUP = 0x0101;

        protected bool HasPatience
        {
            get
            {
                return Core.Player.HasAura("Gathering Fortune Up");
            }
        }

        protected bool HasSnagging
        {
            get
            {
                return Core.Player.HasAura("Snagging");
            }
        }

        protected bool HasCollectorsGlove
        {
            get
            {
                return Core.Player.HasAura("Collector's Glove");
            }
        }

        protected bool HasChum
        {
            get
            {
                return Core.Player.HasAura("Chum");
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
            ShuffleFishSpots();

            if (IsBaitWindowOpen && CanDoAbility(Abilities.Bait))
            {
                DoAbility(Abilities.Bait);
            }

            if (CanDoAbility(Abilities.Quit))
            {
                DoAbility(Abilities.Quit);
            }
        }

        protected override void OnDone()
        {
            Thread.Sleep(6000);
            DoAbility(Abilities.Quit);
            isFishing = false;
            isSitting = false;
            CharacterSettings.Instance.UseMount = true;
        }

        protected override void OnResetCachedDone()
        {
            this.isDone = false;
            spotinit = false;
            fishcount = 0;
            isFishing = false;
            isSitting = false;
        }

        protected override Composite CreateBehavior()
        {
            this.fishlimit = GetFishLimit();

            return new PrioritySelector(
                Conditional,
                Blacklist,
                InventoryFull,
                // TODO: GetBait
                OpenBait,
                ApplyBait,
                CheckStealth,
                StateTransitionAlwaysSucceed,
                MoveToFishSpot,
                GoFish(
                    DismountComposite,
                    StopMovingComposite,
                    CheckWeatherComposite,
                    InitFishSpotComposite,
                    CollectablesComposite,
                    MoochComposite,
                    ReleaseComposite,
                    FishCountLimitComposite,
                    SitComposite,
                    CollectorsGloveComposite,
                    SnaggingComposite,
                    PatienceComposite,
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

        protected static Regex FishRegex = new Regex(
            @"You land an{0,1} (.+) measuring (\d{1,4}\.\d) ilms!",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected static Regex BaitRegex = new Regex(
            @"You apply an{0,1} (.+) to your line.",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected static FishResult FishResult = new FishResult();

        private static string currentBait;

        private int baitCount = InventoryManager.FilledSlots.Count(bs => bs.Item.Affinity == 19);

        private bool isSitting;

        private bool isDone;

        private int minfish = 20;

        private int maxfish = 30;

        private int mooch;

        private int fishcount;

        private int amissfish;

        private int fishlimit;

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
        public int MinimumFishPerSpot
        {
            get
            {
                return this.minfish;
            }

            set
            {
                this.minfish = value;
            }
        }

        [DefaultValue(30)]
        [XmlAttribute("MaxFish")]
        public int MaximumFishPerSpot
        {
            get
            {
                return this.maxfish;
            }

            set
            {
                this.maxfish = value;
            }
        }

        [XmlAttribute("Bait")]
        public string Bait { get; set; }

        [DefaultValue(1)]
        [XmlAttribute("BaitDelay")]
        public int BaitDelay { get; set; }

        [XmlAttribute("Chum")]
        public bool Chum { get; set; }

        [DefaultValue(VirtualKeys.N0)]
        [XmlAttribute("ConfirmKey")]
        public VirtualKeys ConfirmKey { get; set; }

        [DefaultValue(VirtualKeys.N6)]
        [XmlAttribute("MoveCursorRightKey")]
        public VirtualKeys MoveCursorRightKey { get; set; }

        [DefaultValue("True")]
        [XmlAttribute("Condition")]
        public string Condition { get; set; }

        [XmlAttribute("Weather")]
        public string Weather { get; set; }

        [XmlAttribute("ShuffleFishSpots")]
        public bool Shuffle { get; set; }

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
                return new Version(3, 0, 5);
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
                return
                    new Decorator(
                        ret =>
                        Collect && SelectYesNoItem.IsOpen,
                        new Sequence(
                            new Sleep(2, 3),
                            new Action(
                                r =>
                                    {
                                        if (InventoryManager.FilledSlots.Count(c => c.BagId != InventoryBagId.KeyItems)
                                            > 98)
                                        {
                                            Log("Declining Collectible - Only 1 inventory space available", Colors.Red);
                                            return;
                                        }

                                        uint value = 0;
                                        value = SelectYesNoItem.CollectabilityValue;

                                        if (value < 10)
                                        {
                                            new Sleep(2, 3);
                                        }

                                        value = SelectYesNoItem.CollectabilityValue;
                                        Log(
                                            string.Format(
                                                "Collectible caught with value: {0} required: {1}",
                                                value.ToString(),
                                                CollectabilityValue));
                                        if (value >= CollectabilityValue || value < 10)
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
                            new Sleep(2, 3)));
            }
        }

        protected Composite FishCountLimitComposite
        {
            get
            {
                return
                    new Decorator(
                        ret =>
                        fishcount >= fishlimit && CanDoAbility(Abilities.Quit) && !HasPatience
                        && !SelectYesNoItem.IsOpen,
                        new Action(
                            r =>
                                {
                                    DoAbility(Abilities.Quit);
                                    ChangeFishSpot();
                                }));
            }
        }

        protected Composite SitComposite
        {
            get
            {
                return
                    new Decorator(
                        ret =>
                        !isSitting && (Sit || FishSpots.CurrentOrDefault.Sit)
                        && !(FishingManager.State == FishingState.None || FishingManager.State == FishingState.Quit),
                        new Action(
                            r =>
                                {
                                    isSitting = true;
                                    Log("Sitting " + FishSpots.CurrentOrDefault);
                                    ChatManager.SendChat("/sit");
                                }));
            }
        }

        protected Composite StopMovingComposite
        {
            get
            {
                return new Decorator(
                    ret => MovementManager.IsMoving,
                    new Action(r => { MovementManager.MoveForwardStop(); }));
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
                        CanDoAbility(Abilities.Mooch) && MoochLevel != 0 && mooch < MoochLevel && MoochConditionCheck(),
                        new Sequence(
                            new Action(
                                r =>
                                    {
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
                            new Sleep(2, 3)));
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

        protected Composite ReleaseComposite
        {
            get
            {
                return
                    new Decorator(
                        ret => FishingManager.State == FishingState.PoleReady && CanDoAbility(Abilities.Release),
                        new Action(
                            r =>
                                {
                                    // Keep the fish
                                    if (this.Keepers.Count == 0 || this.Keepers.Any(FishResult.IsKeeper)
                                        || (CanDoAbility(Abilities.Mooch) && MoochLevel != 0))
                                        // Do not toss an HQ fish when mooch is active, even if the condition isn't met to currently mooch.
                                    {
                                        if (Chum && !HasChum && CanDoAbility(Abilities.Chum))
                                        {
                                            DoAbility(Abilities.Chum);
                                            new Sleep(1, 2);
                                        }

                                        FishingManager.Cast();
                                        return;
                                    }

                                    Log("Released " + FishResult.FishName);

                                    // Release the fish
                                    DoAbility(Abilities.Release);

                                    ResetMooch();
                                }));
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
                        new Action(
                            r =>
                                {
                                    FishingManager.Cast();
                                    ResetMooch();
                                }));
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

        #endregion

        #region Composites

        protected Composite Conditional
        {
            get
            {
                return new Decorator(ret => !ConditionCheck(), new Action(r => { this.isDone = true; }));
            }
        }

        protected Composite Blacklist
        {
            get
            {
                return new Decorator(
                    ret => amissfish > FishSpots.Count,
                    new Action(
                        r =>
                            {
                                Log("The fish are amiss at all of the FishSpots.");
                                Log(
                                    "This zone has been blacklisted, please fish somewhere else and then restart the profile.");
                                this.isDone = true;
                            }));
            }
        }

        protected Composite InventoryFull
        {
            get
            {
                return
                    new Decorator(
                        ret => InventoryManager.FilledSlots.Count(c => c.BagId != InventoryBagId.KeyItems) >= 100,
                        new Action(r => { this.isDone = true; }));
            }
        }

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
                return RaptureAtkUnitManager.Controls.Any(c => c.Name == "Bait");
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

        protected Composite OpenBait
        {
            get
            {
                return
                    new Decorator(
                        ret =>
                        IsBaitSpecified && !IsCorrectBaitSelected && !IsBaitWindowOpen && CanDoAbility(Abilities.Bait),
                        new Sequence(
                            new Action(
                                r =>
                                    {
                                        DoAbility(Abilities.Bait);
                                        PostKeyPress(this.MoveCursorRightKey);
                                    }),
                            new Sleep(this.BaitDelay)));
            }
        }

        protected Composite ApplyBait
        {
            get
            {
                return new Decorator(
                    ret => IsBaitSpecified && IsBaitWindowOpen && HasSpecifiedBait,
                    new Sequence(
                        new Sleep(this.BaitDelay),
                        new Action(
                            r =>
                                {
                                    if (IsCorrectBaitSelected)
                                    {
                                        Log("Correct Bait Selected -> " + this.Bait);
                                        DoAbility(Abilities.Bait);
                                        return;
                                    }

                                    PostKeyPress(this.MoveCursorRightKey);
                                    Thread.Sleep(100);

                                    PostKeyPress(this.ConfirmKey);
                                    Thread.Sleep(100);

                                    PostKeyPress(this.ConfirmKey);

                                    if (baitCount < -1 && !IsCorrectBaitSelected)
                                    {
                                        Log("Unable to find specified bait -> " + this.Bait + ", ending profile");
                                        this.isDone = true;
                                    }
                                }),
                        new Sleep(1, 2)));
            }
        }

        protected Composite CheckStealth
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

        #endregion

        #region Ability Checks and Actions

        protected bool CanDoAbility(Abilities ability)
        {
            return Actionmanager.CanCast((uint)ability, Core.Player);
        }

        protected void DoAbility(Abilities ability)
        {
            Actionmanager.DoAction((uint)ability, Core.Player);
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
            spotinit = false;
            isFishing = false;
            isSitting = false;
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

        protected static string GetCurrentBait(string message)
        {
            if (BaitRegex.IsMatch(message))
            {
                var match = BaitRegex.Match(message);

                return match.Groups[1].Value;
            }

            return "Parse Error";
        }

        protected static void SetFishResult(string message)
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
        }

        protected void ReceiveMessage(object sender, ChatEventArgs e)
        {
            if (e.ChatLogEntry.MessageType == MessageType.SystemMessages
                && e.ChatLogEntry.Contents.StartsWith("You apply"))
            {
                currentBait = GetCurrentBait(e.ChatLogEntry.Contents);
                this.baitCount--;
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
                DoAbility(Abilities.Quit);
                ChangeFishSpot();
            }

            if (e.ChatLogEntry.MessageType == (MessageType)2115
                && e.ChatLogEntry.Contents
                == "The fish sense something amiss. Perhaps it is time to try another location.")
            {
                Log("The fish sense something amiss!");
                amissfish++;
                DoAbility(Abilities.Quit);
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