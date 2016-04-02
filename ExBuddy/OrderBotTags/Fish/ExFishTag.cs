namespace ExBuddy.OrderBotTags.Fish
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Media;
	using Buddy.Coroutines;
	using Clio.Common;
	using Clio.Utilities;
	using Clio.XmlEngine;
	using ExBuddy.Attributes;
	using ExBuddy.Enumerations;
	using ExBuddy.Helpers;
	using ExBuddy.OrderBotTags.Behaviors;
	using ExBuddy.OrderBotTags.Objects;
	using ff14bot;
	using ff14bot.Behavior;
	using ff14bot.Enums;
	using ff14bot.Managers;
	using ff14bot.Objects;
	using ff14bot.RemoteWindows;
	using ff14bot.Settings;
	using TreeSharp;
	using Action = TreeSharp.Action;

	[LoggerName("ExFish")]
	[XmlElement("ExFish")]
	[XmlElement("Fish")]
	public class ExFishTag : ExProfileBehavior
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

		private readonly Windows.Bait baitWindow = new Windows.Bait();

		internal SpellData CordialSpellData;

		protected override Color Info
		{
			get { return Colors.Gold; }
		}

		public static bool IsFishing()
		{
			return isFishing;
		}

		protected override Composite CreateBehavior()
		{
			fishlimit = GetFishLimit();

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
					new ExCoroutineAction(ctx => HandleBait(), this),
					InitFishSpotComposite,
					new ExCoroutineAction(ctx => HandleCollectable(), this),
					ReleaseComposite,
					MoochComposite,
					FishCountLimitComposite,
					InventoryFullComposite,
					SitComposite,
					CollectorsGloveComposite,
					SnaggingComposite,
					new ExCoroutineAction(ctx => HandleCordial(), this),
					PatienceComposite,
					FishEyesComposite,
					ChumComposite,
					CastComposite,
					HookComposite));
		}

		protected virtual void DoCleanup()
		{
			try
			{
				GamelogManager.MessageRecevied -= ReceiveMessage;
			}
			catch (Exception ex)
			{
				Logger.Error(ex.Message);
			}

			isFishing = false;

			CharacterSettings.Instance.UseMount = initialMountSetting;
		}

		protected override void DoReset()
		{
			mooch = 0;
			sitRoll = 1.0;
			spotinit = false;
			fishcount = 0;
			amissfish = 0;
			isFishing = false;
			isSitting = false;
			isFishIdentified = false;
			fishlimit = GetFishLimit();
			checkRelease = false;

			// Temp fix, only set it to true if it was initially true. Need to find out why this value is false here when it shouldn't be.
			if (initialMountSetting)
			{
				CharacterSettings.Instance.UseMount = initialMountSetting;
			}
		}

		protected Composite GoFish(params Composite[] children)
		{
			return
				new PrioritySelector(
					new Decorator(
						ret => Vector3.Distance(ExProfileBehavior.Me.Location, FishSpots.CurrentOrDefault.Location) < 2,
						new PrioritySelector(children)));
		}

		protected override Task<bool> Main()
		{
			throw new NotImplementedException();
		}

		protected override void OnDone()
		{
			TreeRoot.OnStop -= cleanup;
			DoCleanup();
		}

		protected override void OnStart()
		{
			BaitDelay = BaitDelay.Clamp(0, 5000);

			Item baitItem = null;
			if (BaitId > 0)
			{
				baitItem = DataManager.ItemCache[BaitId];
			}
			else if (!string.IsNullOrWhiteSpace(Bait))
			{
				baitItem =
					DataManager.ItemCache.Values.Find(
						i =>
							string.Equals(i.EnglishName, Bait, StringComparison.InvariantCultureIgnoreCase)
							|| string.Equals(i.CurrentLocaleName, Bait, StringComparison.InvariantCultureIgnoreCase));

				if (baitItem == null)
				{
					isDone = true;
#if RB_CN
                    Logger.Error("寻找鱼饵 '{0}' 出错, 不匹配数据库中的任何数据 " + Bait);
#else
                    Logger.Error("Error finding '{0}', doesn't match any item in the database. " + Bait);
#endif
                    return;
				}
			}

			if (baitItem != null)
			{
				if (Baits == null)
				{
					Baits = new List<Bait>();
				}
#if RB_CN
                Baits.Insert(0, new Bait {Id = baitItem.Id, Name = baitItem.ChnName, BaitItem = baitItem, Condition = "True"});
#else
                Baits.Insert(0, new Bait {Id = baitItem.Id, Name = baitItem.EnglishName, BaitItem = baitItem, Condition = "True"});
#endif
            }

			if (baitItem != null && baitItem.Affinity != 19)
			{
				isDone = true;
#if RB_CN
                Logger.Error("错误: '{0}' 不是鱼饵.", baitItem.ChnName);
#else
                Logger.Error("Error: '{0}' is not considered bait.", baitItem.EnglishName);
#endif
                return;
			}

			if (Keepers == null)
			{
				Keepers = new List<Keeper>();
			}

			if (Collect && Collectables == null)
			{
				Collectables = new List<Collectable> {new Collectable {Name = string.Empty, Value = (int) CollectabilityValue}};
			}

			GamelogManager.MessageRecevied += ReceiveMessage;
			FishSpots.IsCyclic = true;
			isFishing = false;
			isSitting = false;
			initialMountSetting = CharacterSettings.Instance.UseMount;
			ShuffleFishSpots();

			sitRoll = SitRng.NextDouble();

			if (CanDoAbility(Abilities.Quit))
			{
				DoAbility(Abilities.Quit);
			}

			CordialSpellData = DataManager.GetItem((uint) CordialType.Cordial).BackingAction;

			cleanup = bot =>
			{
				DoCleanup();
				TreeRoot.OnStop -= cleanup;
			};

			TreeRoot.OnStop += cleanup;
		}

		internal bool CanUseCordial(ushort withinSeconds = 5)
		{
			return CordialSpellData.Cooldown.TotalSeconds < withinSeconds && !HasChum && !HasPatience && !HasFishEyes
			       && ((CordialType == CordialType.Cordial && Cordial.HasCordials())
			           || CordialType > CordialType.Cordial && Cordial.HasAnyCordials());
		}

		private async Task<bool> HandleBait()
		{
			if (!IsBaitSpecified || IsCorrectBaitSelected)
			{
				// we don't need to worry about bait. Either not specified, or we already have the correct bait selected.
				return false;
			}

			if (FishingManager.State != FishingState.None && FishingManager.State != FishingState.PoleReady)
			{
				// we are not in the proper state to modify our bait. continue.
				return false;
			}

			if (!HasSpecifiedBait)
			{
#if RB_CN
                Logger.Error("你缺少指定的鱼饵: " + Bait);
#else
                Logger.Error("You do not have the specified bait: " + Bait);
#endif
                return isDone = true;
			}

			var baitItem = Fish.Bait.FindMatch(Baits).BaitItem;

			if (!await baitWindow.SelectBait(baitItem.Id, (ushort) BaitDelay))
            {
#if RB_CN
                Logger.Error("选择鱼饵时发生错误");
#else
                Logger.Error("An error has occurred during bait selection.");
#endif
                return isDone = true;
			}
#if RB_CN
            Logger.Info("使用鱼饵 -> " + baitItem.CurrentLocaleName);
#else
            Logger.Info("Using bait -> " + baitItem.EnglishName);
#endif

            return true;
		}

		private async Task<bool> HandleCollectable()
		{
			if (Collectables == null)
			{
				//we are not collecting
				return false;
			}

			if (FishingManager.State != FishingState.Waitin)
			{
				// we are not waitin yet!
				return false;
			}

			if (!SelectYesNoItem.IsOpen)
			{
				//Wait a few seconds
				var opened = await Coroutine.Wait(5000, () => SelectYesNoItem.IsOpen);
				if (!opened)
				{
					return false;
				}
			}

			var required = CollectabilityValue;
			var itemName = string.Empty;
			if (!string.IsNullOrWhiteSpace(Collectables.First().Name))
			{
				var item = SelectYesNoItem.Item;
				if (item == null
				    || !Collectables.Any(c => string.Equals(c.Name, item.EnglishName, StringComparison.InvariantCultureIgnoreCase)))
				{
					var ticks = 0;
					while ((item == null
					        ||
					        !Collectables.Any(c => string.Equals(c.Name, item.EnglishName, StringComparison.InvariantCultureIgnoreCase)))
					       && ticks++ < 60 && Behaviors.ShouldContinue)
					{
						item = SelectYesNoItem.Item;
						await Coroutine.Yield();
					}

					// handle timeout
					if (ticks > 60)
					{
						required = (uint) Collectables.Select(c => c.Value).Max();
					}
				}

				if (item != null)
				{
					// handle normal
					itemName = item.EnglishName;
					var collectable = Collectables.FirstOrDefault(c => string.Equals(c.Name, item.EnglishName));

					if (collectable != null)
					{
						required = (uint) collectable.Value;
					}
				}
			}

			// handle

			var value = SelectYesNoItem.CollectabilityValue;

			if (value >= required)
			{
#if RB_CN
                Logger.Info("确认收藏品： {0} -> 当前价值: {1}, 收藏最低价值: {2}", itemName, value, required);
#else
                Logger.Info("Collecting {0} -> Value: {1}, Required: {2}", itemName, value, required);
#endif
                SelectYesNoItem.Yes();
            }
			else
            {
#if RB_CN
                Logger.Info("取消收藏品 {0} -> 当前价值: {1}, 收藏最低价值: {2}", itemName, value, required);
#else
                Logger.Info("Declining {0} -> Value: {1}, Required: {2}", itemName, value, required);
#endif
                SelectYesNoItem.No();
			}

			await Coroutine.Wait(3000, () => !SelectYesNoItem.IsOpen && FishingManager.State != FishingState.Waitin);

			return true;
		}

		private async Task<bool> HandleCordial()
		{
			if (CordialType == CordialType.None)
			{
				// Not using cordials, skip method.
				return false;
			}

			if (FishingManager.State >= FishingState.Bite)
			{
				// Need to wait till we are in the correct state
				return false;
			}

			CordialSpellData = CordialSpellData ?? Cordial.GetSpellData();

			if (CordialSpellData == null)
			{
				CordialType = CordialType.None;
				return false;
			}

			if (!CanUseCordial(8))
			{
				// has a buff or cordial cooldown not ready or we have no cordials.
				return false;
			}

			var missingGp = ExProfileBehavior.Me.MaxGP - ExProfileBehavior.Me.CurrentGP;

			if (missingGp < 300 && !ForceCordial)
			{
				// Not forcing cordial and less than 300gp missing from max.
				return false;
			}

			await Coroutine.Wait(10000, () => CanDoAbility(Abilities.Quit));
			DoAbility(Abilities.Quit);
			isSitting = false;

			await Coroutine.Wait(5000, () => FishingManager.State == FishingState.None);

			if (missingGp >= 380 && CordialType >= CordialType.HiCordial)
			{
				if (await UseCordial(CordialType.HiCordial))
				{
					return true;
				}
			}

			if (await UseCordial(CordialType.Cordial))
			{
				return true;
			}

			return false;
		}

		private async Task<bool> UseCordial(CordialType cordialType, int maxTimeoutSeconds = 5)
		{
			if (CordialSpellData.Cooldown.TotalSeconds < maxTimeoutSeconds)
			{
				var cordial = InventoryManager.FilledSlots.FirstOrDefault(slot => slot.RawItemId == (uint) cordialType);

				if (cordial != null)
				{
					StatusText = "Using cordial when it becomes available";

#if RB_CN
                    Logger.Info(
						"使用强心剂 -> 等待时间 (秒): {0}, 当前采集力: {1}",
						(int) CordialSpellData.Cooldown.TotalSeconds,
						ExProfileBehavior.Me.CurrentGP);
#else
                    Logger.Info(
						"Using Cordial -> Waiting (sec): {0}, CurrentGP: {1}",
						(int) CordialSpellData.Cooldown.TotalSeconds,
						ExProfileBehavior.Me.CurrentGP);
#endif

                    if (await Coroutine.Wait(
						TimeSpan.FromSeconds(maxTimeoutSeconds),
						() =>
						{
							if (ExProfileBehavior.Me.IsMounted && CordialSpellData.Cooldown.TotalSeconds < 2)
							{
								Actionmanager.Dismount();
								return false;
							}

							return cordial.CanUse(ExProfileBehavior.Me);
						}))
					{
						await Coroutine.Sleep(500);
#if RB_CN
                        Logger.Info("使用 " + cordialType);
#else
                        Logger.Info("Using " + cordialType);
#endif
                        cordial.UseItem(ExProfileBehavior.Me);
						await Coroutine.Sleep(1500);
						return true;
					}
				}
				else
				{
#if RB_CN
                    Logger.Warn("没有强心剂,你需要购买更多的 " + cordialType);
#else
                    Logger.Warn("No Cordial avilable, buy more " + cordialType);
#endif
                }
			}

			return false;
		}

#region Aura Properties

		protected bool HasPatience
		{
			get
			{
				// Gathering Fortune Up (Fishing)
				return ExProfileBehavior.Me.HasAura(850);
			}
		}

		protected bool HasSnagging
		{
			get
			{
				// Snagging
				return ExProfileBehavior.Me.HasAura(761);
			}
		}

		protected bool HasCollectorsGlove
		{
			get
			{
				// Collector's Glove
				return ExProfileBehavior.Me.HasAura(805);
			}
		}

		protected bool HasChum
		{
			get
			{
				// Chum
				return ExProfileBehavior.Me.HasAura(763);
			}
		}

		protected bool HasFishEyes
		{
			get
			{
				// Fish Eyes
				return ExProfileBehavior.Me.HasAura(762);
			}
		}

#endregion

#region Fields

		private static bool isFishing;

		protected static readonly Random SitRng = new Random();

#if RB_CN
       protected static Regex FishRegex = new Regex(
            @"[\u4e00-\u9fa5A-Za-z0-9]+成功钓上了|[\u4e00-\u9fa5A-Za-z0-9]+",
            //@"[\u0800-\u4e00A-Za-z0-9]+は|[\u0800-\u4e00]+",    for Japanese
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected static Regex FishSizeRegex = new Regex(
            @"(\d{1,4}\.\d)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
#else
        protected static Regex FishRegex = new Regex(
			@"You land an{0,1} (.+) measuring (\d{1,4}\.\d) ilms!",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);
#endif

        protected static FishResult FishResult = new FishResult();

		private Func<bool> conditionFunc;

		private Func<bool> moochConditionFunc;

		private bool initialMountSetting;

		private BotEvent cleanup;

		private bool checkRelease;

		private bool isSitting;

		private bool isFishIdentified;

		private int mooch;

		private int fishcount;

		private int amissfish;

		private int fishlimit;

		private double sitRoll = 1.0;

		private bool spotinit;

#endregion

#region Public Properties

		[XmlElement("Baits")]
		public List<Bait> Baits { get; set; }

		[DefaultValue(CordialType.None)]
		[XmlAttribute("CordialType")]
		public CordialType CordialType { get; set; }

		[XmlAttribute("ForceCordial")]
		public bool ForceCordial { get; set; }

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

		[XmlAttribute("BaitId")]
		public uint BaitId { get; set; }

		[DefaultValue(200)]
		[XmlAttribute("BaitDelay")]
		public int BaitDelay { get; set; }

		[XmlAttribute("Chum")]
		public bool Chum { get; set; }

		[DefaultValue(30)]
		[XmlAttribute("LastFishTimeout")]
		public int LastFishTimeout { get; set; }

		[DefaultValue("True")]
		[XmlAttribute("Condition")]
		public string Condition { get; set; }

		[XmlAttribute("Weather")]
		public string Weather { get; set; }

		[DefaultValue(2.0f)]
		[XmlAttribute("Radius")]
		public float Radius { get; set; }

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
		public uint CollectabilityValue { get; set; }

		[DefaultValue(Abilities.None)]
		[XmlAttribute("Patience")]
		public Abilities Patience { get; set; }

		[XmlAttribute("FishEyes")]
		public bool FishEyes { get; set; }

		[XmlAttribute("Snagging")]
		public bool Snagging { get; set; }

		[XmlElement("PatienceTugs")]
		public List<PatienceTug> PatienceTugs { get; set; }

#endregion

#region Private Properties

		internal bool MovementStopCallback(float distance, float radius)
		{
			return distance <= radius || !ConditionCheck() || ExProfileBehavior.Me.IsDead;
		}

		private bool HasSpecifiedBait
		{
			get { return Fish.Bait.FindMatch(Baits).BaitItem.ItemCount() > 0; }
		}

		private bool IsBaitSpecified
		{
			get { return Baits != null && Baits.Count > 0; }
		}

		private bool IsCorrectBaitSelected
		{
			get { return Fish.Bait.FindMatch(Baits).BaitItem.Id == FishingManager.SelectedBaitItemId; }
		}

#endregion

#region Fishing Composites

		protected Composite DismountComposite
		{
			get { return new Decorator(ret => ExProfileBehavior.Me.IsMounted, CommonBehaviors.Dismount()); }
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
							!isSitting && (Sit || FishSpots.CurrentOrDefault.Sit || sitRoll < SitRate)
							&& FishingManager.State == (FishingState) 9,
						// this is when you have already cast and are waiting for a bite.
						new Sequence(
							new Sleep(1, 1),
							new Action(
								r =>
								{
									isSitting = true;
#if RB_CN
                                    Logger.Info("坐下 " + FishSpots.CurrentOrDefault);
#else
                                    Logger.Info("Sitting " + FishSpots.CurrentOrDefault);
#endif
                                    ChatManager.SendChat("/sit");
								})));
			}
		}

		protected Composite StopMovingComposite
		{
			get { return new Decorator(ret => MovementManager.IsMoving, CommonBehaviors.MoveStop()); }
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
#if RB_CN
                            Logger.Info("在移动换渔点前会再钓 " + fishlimit + " 次。");
#else
                            Logger.Info("Will fish for " + fishlimit + " fish before moving again.");
#endif
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
#if RB_CN
                        new Action(r => { Logger.Info("等待正确天气..."); }),
#else
                        new Action(r => { Logger.Info("Waiting for the proper weather..."); }),
#endif
                        new Wait(36000, ret => Weather == WorldManager.CurrentWeather, new ActionAlwaysSucceed())));
			}
		}

		protected Composite CollectorsGloveComposite
		{
			get
			{
				return new Decorator(
					ret => CanDoAbility(Abilities.CollectorsGlove) && Collectables != null ^ HasCollectorsGlove,
					new Sequence(
						new Action(
							r =>
							{
#if RB_CN
                                    Logger.Info("使用[收藏品采集]");
#else
                                Logger.Info("Casting Collector's Glove");
#endif
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
#if RB_CN
                                Logger.Info("开启钓组");
#else
                                Logger.Info("Toggle Snagging");
#endif
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
							CanDoAbility(Abilities.Mooch) && MoochLevel != 0 && mooch < MoochLevel && MoochConditionCheck()
							&& (Keepers.Count == 0
							    || Keepers.All(k => !string.Equals(k.Name, FishResult.FishName, StringComparison.InvariantCultureIgnoreCase))
							    || Keepers.Any(
								    k =>
									    string.Equals(k.Name, FishResult.FishName, StringComparison.InvariantCultureIgnoreCase)
									    && FishResult.ShouldMooch(k))),
						new Sequence(
							new Action(
								r =>
								{
									checkRelease = true;
									FishingManager.Mooch();
									mooch++;
									if (MoochLevel > 1)
									{
#if RB_CN
                                        Logger.Info("以小钓大中, 这是 " + mooch + " 次，一共 " + MoochLevel + " 次.");
#else
                                        Logger.Info("Mooching, this is mooch " + mooch + " of " + MoochLevel + " mooches.");
#endif
                                    }
                                    else
                                    {
#if RB_CN
                                        Logger.Info("以小钓大中, 只有这一次.");
#else
                                        Logger.Info("Mooching, this will be the only mooch.");
#endif
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
							&& (FishingManager.State == FishingState.None || FishingManager.State == FishingState.PoleReady) && !HasPatience
							&& CanDoAbility(Patience) &&
							(ExProfileBehavior.Me.CurrentGP >= 600 || ExProfileBehavior.Me.CurrentGPPercent > 99.0f),
						new Sequence(
							new Action(
								r =>
								{
									DoAbility(Patience);
#if RB_CN
                                    Logger.Info("启用“耐心”");
#else
                                    Logger.Info("Patience activated");
#endif
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
							checkRelease && FishingManager.State == FishingState.PoleReady && CanDoAbility(Abilities.Release)
							&& Keepers.Count != 0,
						new Sequence(
							new Wait(
								2,
								ret => isFishIdentified,
								new Action(
									r =>
									{
                                        // If its not a keeper AND we aren't mooching or we can't mooch, then release
#if RB_CN
                                        Logger.Info("钓上来了：{0},尺寸：{1}星寸", FishResult.Name, FishResult.Size);
                                        if (!Keepers.Any(FishResult.IsKeeper) && (MoochLevel == 0 || !CanDoAbility(Abilities.Mooch)))
                                        {
                                            DoAbility(Abilities.Release);
                                            Logger.Info("放生：{0},尺寸：{1}星寸", FishResult.Name, FishResult.Size);
                                        }
#else
                                        if (!Keepers.Any(FishResult.IsKeeper) && (MoochLevel == 0 || !CanDoAbility(Abilities.Mooch)))
										{
											DoAbility(Abilities.Release);
											Logger.Info("Released " + FishResult.Name);
										}
#endif
                                        checkRelease = false;
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
						ret => FishingManager.State == FishingState.None || FishingManager.State == FishingState.PoleReady,
						new Action(r => Cast()));
			}
		}

		protected Composite InventoryFullComposite
		{
			get
			{
				return new Decorator(
					// TODO: Log reason for quit.
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
							var tugType = FishingManager.TugType;
							var patienceTug = new PatienceTug {MoochLevel = mooch, TugType = tugType};
							var hookset = tugType == TugType.Light ? Abilities.PrecisionHookset : Abilities.PowerfulHookset;
							if (HasPatience && CanDoAbility(hookset) && (PatienceTugs == null || PatienceTugs.Contains(patienceTug)))
							{
								DoAbility(hookset);
#if RB_CN
                                Logger.Info("检测到{0}杆. 使用 {1}", tugType, hookset);
#else
                                Logger.Info("{0} TugType detected. Using {1}", tugType, hookset);
#endif
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
#if RB_CN
                            Logger.Info("在当前渔点钓了 " + fishcount + " 次，一共 " + fishlimit + " 次。");
#else
                            Logger.Info("Fished " + fishcount + " of " + fishlimit + " fish at this FishSpot.");
#endif
                        }));
			}
		}

		protected Composite CheckStealthComposite
		{
			get
			{
				return new Decorator(
					ret => Stealth && !ExProfileBehavior.Me.HasAura(47),
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

#endregion

#region Composites

		protected Composite Conditional
		{
			get { return new Decorator(ret => FishingManager.State < FishingState.Bite && !ConditionCheck(), IsDoneAction); }
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
#if RB_CN                                
                                Logger.Warn("这块地图的鱼都警惕了。");
                                Logger.Warn("这块地图的鱼都警惕了，请在其他地图钓一下鱼再重新运行脚本。");
#else
                                Logger.Warn("The fish are amiss at all of the FishSpots.");
								Logger.Warn("This zone has been blacklisted, please fish somewhere else and then restart the profile.");
#endif
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
					ret => Vector3.Distance(ExProfileBehavior.Me.Location, FishSpots.CurrentOrDefault.Location) > 1,
					CommonBehaviors.MoveAndStop(ret => FishSpots.CurrentOrDefault.Location, 1, true));
			}
		}

		protected Composite IsDoneAction
		{
			get
			{
				return
					new Sequence(
						new WaitContinue(
							LastFishTimeout,
							ret => FishingManager.State < FishingState.Bite,
							new Sequence(
								new PrioritySelector(
									new ExCoroutineAction(ctx => HandleCollectable(), this),
									ReleaseComposite,
									new ActionAlwaysSucceed()),
								new Sleep(2, 3),
								new Action(r => DoAbility(Abilities.Quit)),
								new Sleep(2, 3),
								new Action(r => { isDone = true; }))));
			}
		}

#endregion

#region Ability Checks and Actions

		internal bool CanDoAbility(Abilities ability)
		{
			return Actionmanager.CanCast((uint) ability, ExProfileBehavior.Me);
		}

		internal bool DoAbility(Abilities ability)
		{
			return Actionmanager.DoAction((uint) ability, ExProfileBehavior.Me);
		}

#endregion

#region Methods

		protected virtual bool ConditionCheck()
		{
			if (conditionFunc == null)
			{
				conditionFunc = ScriptManager.GetCondition(Condition);
			}

			return conditionFunc();
		}

		protected virtual bool MoochConditionCheck()
		{
			if (moochConditionFunc == null)
			{
				moochConditionFunc = ScriptManager.GetCondition(MoochCondition);
			}

			return moochConditionFunc();
		}

		protected virtual void Cast()
		{
			isFishIdentified = false;
			checkRelease = true;
			FishingManager.Cast();
			ResetMooch();
		}

		protected virtual void FaceFishSpot()
		{
			var i = MathEx.Random(0, 25);
			i = i/100;

			var i2 = MathEx.Random(0, 100);

			if (i2 > 50)
			{
				ExProfileBehavior.Me.SetFacing(FishSpots.Current.Heading - (float) i);
			}
			else
			{
				ExProfileBehavior.Me.SetFacing(FishSpots.Current.Heading + (float) i);
			}
		}

		protected virtual void ChangeFishSpot()
		{
			FishSpots.Next();
#if RB_CN
            Logger.Info("更换渔点...");
#else
            Logger.Info("Changing FishSpots...");
#endif
            fishcount = 0;
#if RB_CN
            Logger.Info("重置钓鱼计数...");
#else
            Logger.Info("Resetting fish count...");
#endif
            fishlimit = GetFishLimit();
			sitRoll = SitRng.NextDouble();
			spotinit = false;
			isFishing = false;
			isSitting = false;
		}

		protected virtual int GetFishLimit()
		{
			return Convert.ToInt32(MathEx.Random(MinimumFishPerSpot, MaximumFishPerSpot));
		}

		protected void ShuffleFishSpots()
		{
			if (Shuffle && FishSpots.Index == 0)
			{
				FishSpots.Shuffle();
#if RB_CN
                Logger.Info("打乱渔点顺序");
#else
                Logger.Info("Shuffled fish spots");
#endif
            }
        }

		protected void ResetMooch()
		{
			if (mooch != 0)
			{
				mooch = 0;
#if RB_CN
                Logger.Info("重置以小钓大级别");
#else
                Logger.Info("Resetting mooch level.");
#endif
            }
        }

		protected void SetFishResult(string message)
		{
#if RB_CN
            var fishResult = new FishResult();

            var match = FishRegex.Matches(message);
            var sizematch = FishSizeRegex.Match(message);

            if (sizematch.Success)
            {
                fishResult.Name = match[1].ToString();
                float size;
                float.TryParse(sizematch.Groups[1].Value, out size);
                fishResult.Size = size;

                if (fishResult.Name[fishResult.Name.Length - 2] == ' ')
                {
                    fishResult.IsHighQuality = true;
                }
            }

#else
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
#endif
            FishResult = fishResult;
			isFishIdentified = true;
		}

		protected void ReceiveMessage(object sender, ChatEventArgs e)
		{
#if RB_CN
            if (e.ChatLogEntry.MessageType == (MessageType)2115 && e.ChatLogEntry.Contents.Contains("成功钓上了"))
            {
                SetFishResult(e.ChatLogEntry.Contents);
            }
#else
            if (e.ChatLogEntry.MessageType == (MessageType) 2115 && e.ChatLogEntry.Contents.StartsWith("You land"))
			{
				SetFishResult(e.ChatLogEntry.Contents);
			}
#endif

            if (e.ChatLogEntry.MessageType == (MessageType) 2115
			    && e.ChatLogEntry.Contents.Equals("You do not sense any fish here.", StringComparison.InvariantCultureIgnoreCase))
			{
				Logger.Info("You do not sense any fish here, trying next location.");

				if (CanDoAbility(Abilities.Quit))
				{
					DoAbility(Abilities.Quit);
				}

				ChangeFishSpot();
			}

			if (e.ChatLogEntry.MessageType == (MessageType)2115
#if RB_CN
                && e.ChatLogEntry.Contents == "这里的鱼现在警惕性很高，看来还是换个地点比较好。")
#else
                && e.ChatLogEntry.Contents == "The fish sense something amiss. Perhaps it is time to try another location.")
#endif
            {
#if RB_CN
                Logger.Info("鱼警惕了，换点位!");
#else
                Logger.Info("The fish sense something amiss!");
#endif
                amissfish++;

				if (CanDoAbility(Abilities.Quit))
				{
					DoAbility(Abilities.Quit);
				}

				ChangeFishSpot();
			}
		}

#endregion
	}
}