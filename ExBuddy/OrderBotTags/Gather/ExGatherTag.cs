namespace ExBuddy.OrderBotTags.Gather
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using System.Windows.Media;

	using Buddy.Coroutines;

	using Clio.Utilities;
	using Clio.XmlEngine;

	using ExBuddy.Attributes;
	using ExBuddy.Enumerations;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ExBuddy.OrderBotTags.Behaviors;
	using ExBuddy.OrderBotTags.Gather.GatherSpots;
	using ExBuddy.OrderBotTags.Gather.Rotations;
	using ExBuddy.OrderBotTags.Objects;
	using ExBuddy.Windows;

	using ff14bot;
	using ff14bot.Behavior;
	using ff14bot.Enums;
	using ff14bot.Helpers;
	using ff14bot.Managers;
	using ff14bot.Navigation;
	using ff14bot.NeoProfiles;
	using ff14bot.Objects;

	using TreeSharp;

	[LoggerName("ExGather")]
	[XmlElement("ExGather")]
	[XmlElement("GatherCollectable")]
	public sealed class ExGatherTag : ExProfileBehavior
	{
		private static readonly object Lock = new object();

		internal static volatile Dictionary<string, IGatheringRotation> Rotations;

		internal Collectable CollectableItem;

		internal SpellData CordialSpellData;

		internal GatheringItem GatherItem;

		internal bool GatherItemIsFallback;

		internal IGatherSpot GatherSpot;

		internal GatheringPointObject Node;

		internal int NodesGatheredAtMaxGp;

		internal Func<bool> WhileFunc;

		private Func<bool> freeRangeConditionFunc;

		private IGatheringRotation gatherRotation;

		private IGatheringRotation initialGatherRotation;

		private bool interactedWithNode;

		private int loopCount;

		private Composite poiCoroutine;

		private DateTime startTime;

		public ExGatherTag()
		{
			if (Rotations == null)
			{
				lock (Lock)
				{
					if (Rotations == null)
					{
						Rotations = LoadRotationTypes();
					}
				}
			}
		}

		public int AdjustedWaitForGp
		{
			get
			{
				var requiredGp = gatherRotation == null ? 0 : gatherRotation.Attributes.RequiredGp;

				// Return the lower of your MaxGP rounded down to the nearest 50.
				return Math.Min(Me.MaxGP - (Me.MaxGP % 50), requiredGp);
			}
		}

		[DefaultValue(true)]
		[XmlAttribute("AlwaysGather")]
		public bool AlwaysGather { get; set; }

		[XmlElement("Collectables")]
		public List<Collectable> Collectables { get; set; }

		[DefaultValue(CordialTime.IfNeeded)]
		[XmlAttribute("CordialTime")]
		public CordialTime CordialTime { get; set; }

		[DefaultValue(CordialType.None)]
		[XmlAttribute("CordialType")]
		public CordialType CordialType { get; set; }

		// TODO: Look into making this use Type instead of Enum
		[DefaultValue(GatherSpotType.GatherSpot)]
		[XmlAttribute("DefaultGatherSpotType")]
		public GatherSpotType DefaultGatherSpotType { get; set; }

		[XmlAttribute("DisableRotationOverride")]
		public bool DisableRotationOverride { get; set; }

		[XmlAttribute("DiscoverUnknowns")]
		public bool DiscoverUnknowns { get; set; }

		[DefaultValue(3.1f)]
		[XmlAttribute("Distance")]
		public float Distance { get; set; }

		[XmlAttribute("FreeRange")]
		public bool FreeRange { get; set; }

		[DefaultValue("Condition.TrueFor(1, TimeSpan.FromHours(1))")]
		[XmlAttribute("FreeRangeCondition")]
		public string FreeRangeCondition { get; set; }

		[DefaultValue(GatherIncrease.Auto)]
		[XmlAttribute("GatherIncrease")]
		public GatherIncrease GatherIncrease { get; set; }

		[XmlElement("GatheringSkillOrder")]
		public GatheringSkillOrder GatheringSkillOrder { get; set; }

		// Backwards compatibility
		[XmlElement("GatherObject")]
		public string GatherObject { get; set; }

		[XmlElement("GatherObjects")]
		public List<string> GatherObjects { get; set; }

		// Maybe this should be an attribute?
		[DefaultValue("RegularNode")]
		[XmlElement("GatherRotation")]
		public string GatherRotation { get; set; }

		[XmlElement("GatherSpots")]
		public GatherSpotCollection GatherSpots { get; set; }

		[DefaultValue(GatherStrategy.GatherOrCollect)]
		[XmlAttribute("GatherStrategy")]
		public GatherStrategy GatherStrategy { get; set; }

		[XmlElement("HotSpots")]
		public IndexedList<HotSpot> HotSpots { get; set; }

		[XmlElement("ItemNames")]
		public List<string> ItemNames { get; set; }

		[DefaultValue(-1)]
		[XmlAttribute("Loops")]
		public int Loops { get; set; }

		[DefaultValue(3.0f)]
		[XmlAttribute("Radius")]
		public float Radius { get; set; }

		[DefaultValue(400)]
		[XmlAttribute("SkipWindowDelay")]
		public int SkipWindowDelay { get; set; }

		// I want this to be an attribute, but for backwards compatibilty, we will use element
		[DefaultValue(-1)]
		[XmlElement("Slot")]
		public int Slot { get; set; }

		[XmlAttribute("SpawnTimeout")]
		public int SpawnTimeout { get; set; }

		[XmlAttribute("SpellDelay")]
		public int SpellDelay { get; set; }

		[DefaultValue("True")]
		[XmlAttribute("While")]
		public string While { get; set; }

		[DefaultValue(1000)]
		[XmlAttribute("WindowDelay")]
		public int WindowDelay { get; set; }

		protected override Color Info
		{
			get
			{
				return Colors.Chartreuse;
			}
		}

		protected override void DoReset()
		{
			loopCount = 0;
			NodesGatheredAtMaxGp = 0;

			if (HotSpots != null)
			{
				HotSpots.Index = 0;
			}

			ResetInternal();
		}

		protected override async Task<bool> Main()
		{
			await CommonTasks.HandleLoading();

			return HandleDeath() || HandleCondition() || await CastTruth() || HandleReset() || await MoveToHotSpot()
					|| await FindNode() || await ResetOrDone();
		}

		protected override void OnDone()
		{
			TreeHooks.Instance.RemoveHook("PoiAction", poiCoroutine);
		}

		protected override void OnStart()
		{
			SpellDelay = SpellDelay < 0 ? 0 : SpellDelay;
			WindowDelay = WindowDelay < 500 ? 500 : WindowDelay;
			SkipWindowDelay = SkipWindowDelay < 200 ? 200 : SkipWindowDelay;

			if (Distance > 3.5f)
			{
				TreeRoot.Stop("Using a distance of greater than 3.5 is not supported, change the value and restart the profile.");
			}

			if (HotSpots != null)
			{
				HotSpots.IsCyclic = Loops < 1;
			}

			// backwards compatibility
			if (GatherObjects == null && !string.IsNullOrWhiteSpace(GatherObject))
			{
				GatherObjects = new List<string> { GatherObject };
			}

			startTime = DateTime.Now;

			CordialSpellData = DataManager.GetItem((uint)CordialType.Cordial).BackingAction;

			if (string.IsNullOrWhiteSpace(Name))
			{
				if (Collectables != null && Collectables.Count > 0)
				{
					Name = Collectables.First().Name;
				}
				else if (ItemNames != null && ItemNames.Count > 0)
				{
					Name = ItemNames.First();
				}
				else
				{
					Name = string.Format("ZoneId [{0}], Calling Location {1}", WorldManager.ZoneId, Core.Player.Location);
				}
			}

			StatusText = Name;

			poiCoroutine = new ActionRunCoroutine(ctx => ExecutePoiLogic());
			TreeHooks.Instance.AddHook("PoiAction", poiCoroutine);

			ResolveGatherRotation();
		}

		internal async Task<bool> Cast(uint id)
		{
			return await Actions.Cast(id, SpellDelay);
		}

		internal async Task<bool> Cast(Ability id)
		{
			return await Actions.Cast(id, SpellDelay);
		}

		internal async Task<bool> CastAura(uint spellId, int auraId = -1)
		{
			return await Actions.CastAura(spellId, SpellDelay, auraId);
		}

		internal async Task<bool> CastAura(Ability ability, AbilityAura auraId = AbilityAura.None)
		{
			return await Actions.CastAura(ability, SpellDelay, auraId);
		}

		internal async Task<bool> CloseGatheringWindow()
		{
			return
				await
				Gathering.CloseGently(
					(byte)(SkipWindowDelay < 33 ? 100 : Math.Max(1, 3000 / SkipWindowDelay)),
					(ushort)SkipWindowDelay);
		}

		internal bool IsConcealed()
		{
			return Node.EnglishName.IndexOf("concealed", StringComparison.InvariantCultureIgnoreCase) >= 0;
		}

		internal bool IsEphemeral()
		{
			return Node.EnglishName.IndexOf("ephemeral", StringComparison.InvariantCultureIgnoreCase) >= 0;
		}

		internal bool IsUnspoiled()
		{
			// Temporary until we decide if legendary have any diff properties or if we should treat them the same.
			return Node.EnglishName.IndexOf("unspoiled", StringComparison.InvariantCultureIgnoreCase) >= 0
					|| Node.EnglishName.IndexOf("legendary", StringComparison.InvariantCultureIgnoreCase) >= 0;
		}

		internal bool MovementStopCallback(float distance, float radius)
		{
			return distance <= radius || !WhileFunc() || Me.IsDead;
		}

		internal void ResetInternal()
		{
			interactedWithNode = false;
			GatherSpot = null;
			Node = null;
			GatherItem = null;
			CollectableItem = null;
		}

		internal async Task<bool> ResolveGatherItem()
		{
			if (!GatheringManager.WindowOpen)
			{
				return false;
			}

			var previousGatherItem = GatherItem;
			GatherItemIsFallback = false;
			GatherItem = null;
			CollectableItem = null;

			var windowItems = GatheringManager.GatheringWindowItems.ToArray();

			// TODO: move method to common so we use it on fish too
			if (InventoryItemCount() >= 100)
			{
				if (ItemNames != null && ItemNames.Count > 0)
				{
					if (
						SetGatherItemByItemName(
							windowItems.OrderByDescending(i => i.SlotIndex).Where(i => i.IsFilled && !i.IsUnknown && i.ItemId < 20).ToArray()))
					{
						return true;
					}
				}

				GatherItem =
					windowItems.Where(i => i.IsFilled && !i.IsUnknown)
						.OrderByDescending(i => i.ItemId)
						.FirstOrDefault(i => i.ItemId < 20);

				if (GatherItem != null)
				{
					return true;
				}

				Logger.Warn("Inventory is full and no shards/crystals/clusters to gather. Gathering complete.");
				return false;
			}

			if (DiscoverUnknowns)
			{
				var items = new[] { 0U, 1U, 2U, 3U, 4U, 5U, 6U, 7U }.Select(GatheringManager.GetGatheringItemByIndex).ToArray();

				GatherItem = items.FirstOrDefault(i => i.IsUnknownChance() && i.Amount > 0);

				if (GatherItem != null)
				{
					return true;
				}
			}

			if (Collectables != null && Collectables.Count > 0)
			{
				foreach (var collectable in Collectables)
				{
					GatherItem =
						windowItems.FirstOrDefault(
							i =>
							i.IsFilled && !i.IsUnknown
							&& string.Equals(collectable.Name, i.ItemData.EnglishName, StringComparison.InvariantCultureIgnoreCase));

					if (GatherItem != null)
					{
						CollectableItem = collectable;
						return true;
					}
				}
			}

			if (ItemNames != null && ItemNames.Count > 0)
			{
				if (SetGatherItemByItemName(windowItems))
				{
					return true;
				}
			}

			if (Slot > -1 && Slot < 8)
			{
				GatherItem = GatheringManager.GetGatheringItemByIndex((uint)Slot);
			}

			if (GatherItem == null && (!AlwaysGather || GatherStrategy == GatherStrategy.TouchAndGo))
			{
				Poi.Clear("Skipping this node, no items we want to gather.");

				await CloseGatheringWindow();

				return false;
			}

			if (GatherItem != null)
			{
				return true;
			}

			GatherItemIsFallback = true;

			GatherItem =
				windowItems.Where(i => i.IsFilled && !i.IsUnknown)
					.OrderByDescending(i => i.ItemId)
					.FirstOrDefault(i => i.ItemId < 20) // Try to gather cluster/crystal/shard
				?? windowItems.FirstOrDefault(
					i => i.IsFilled && !i.IsUnknown && !i.ItemData.Unique && !i.ItemData.Untradeable && i.ItemData.ItemCount() > 0)
				// Try to collect items you have that stack
				?? windowItems.Where(i => i.Amount > 0 && !i.ItemData.Unique && !i.ItemData.Untradeable)
						.OrderByDescending(i => i.SlotIndex)
						.FirstOrDefault(); // Take last item that is not unique or untradeable

			// Seems we only have unknowns.
			if (GatherItem == null)
			{
				var items = new[] { 0U, 1U, 2U, 3U, 4U, 5U, 6U, 7U }.Select(GatheringManager.GetGatheringItemByIndex).ToArray();

				GatherItem = items.FirstOrDefault(i => i.IsUnknownChance() && i.Amount > 0);

				if (GatherItem != null)
				{
					return true;
				}

				Logger.Warn("Unable to find an item to gather, moving on.");

				return false;
			}

			if (previousGatherItem == null || previousGatherItem.ItemId != GatherItem.ItemId)
			{
				Logger.Info("could not find item by slot or name, gathering " + GatherItem.ItemData + " instead.");
			}

			return true;
		}

		private async Task<bool> AfterGather()
		{
			Logger.Verbose("Finished gathering from {0} with {1} GP at {2} ET", Node.EnglishName, Me.CurrentGP, WorldManager.EorzaTime.ToShortTimeString());

			// in case we failed our rotation or window stuck open because items are somehow left
			if (GatheringManager.SwingsRemaining > 0)
			{
				// TODO: Look into possibly smarter behavior.
				await CloseGatheringWindow();
			}

			if (Me.CurrentGP >= Me.MaxGP - 30)
			{
				NodesGatheredAtMaxGp++;
			}
			else
			{
				NodesGatheredAtMaxGp = 0;
			}

			if (!ReferenceEquals(gatherRotation, initialGatherRotation))
			{
				gatherRotation = initialGatherRotation;
				Logger.Info("Rotation reset -> " + initialGatherRotation.Attributes.Name);
			}

			if (CordialTime.HasFlag(CordialTime.AfterGather))
			{
				if (CordialType == CordialType.Auto)
				{
					if (Me.MaxGP - Me.CurrentGP > 550)
					{
						if (await UseCordial(CordialType.HiCordial))
						{
							return true;
						}
					}

					if (Me.MaxGP - Me.CurrentGP > 390)
					{
						if (await UseCordial(CordialType.Cordial))
						{
							return true;
						}
					}
				}

				if (CordialType == CordialType.HiCordial)
				{
					if (Me.MaxGP - Me.CurrentGP > 430)
					{
						if (await UseCordial(CordialType.HiCordial))
						{
							return true;
						}

						if (await UseCordial(CordialType.Cordial))
						{
							return true;
						}
					}
				}

				if (CordialType == CordialType.Cordial && Me.MaxGP - Me.CurrentGP > 330)
				{
					if (await UseCordial(CordialType.Cordial))
					{
						return true;
					}
				}
			}

			return true;
		}

		private async Task<bool> BeforeGather()
		{
			if (Me.CurrentGP >= AdjustedWaitForGp)
			{
				return true;
			}

			var ttg = GetTimeToGather();

			if (ttg.RealSecondsTillStartGathering < 3)
			{
				Logger.Warn("Not enough time to gather, will still make an attempt.");
				return true;
			}

			var gp = Math.Min(Me.CurrentGP + ttg.TicksTillStartGathering * 5, Me.MaxGP);

			if (CordialType <= CordialType.None)
			{
				if (gp >= AdjustedWaitForGp)
				{
					return await WaitForGpRegain();
				}

				Logger.Warn("Gathering without the minimum recommended GP for the rotation");
				Logger.Warn(
					"Cordial not enabled.  To enable cordial use, add the 'cordialType' attribute with value 'Auto', 'Cordial', or 'HiCordial'");

				return true;
			}

			if (gp >= AdjustedWaitForGp)
			{
				if (CordialTime.HasFlag(CordialTime.IfNeeded))
				{
					return await WaitForGpRegain();
				}

				var gpNeeded = AdjustedWaitForGp - (Me.CurrentGP - (Me.CurrentGP % 5));
				var gpNeededTicks = gpNeeded / 5;
				var gpNeededSeconds = gpNeededTicks * 3;

				if (gpNeededSeconds <= CordialSpellData.Cooldown.TotalSeconds + 3)
				{
					Logger.Info("GP recovering faster than cordial cooldown, waiting for GP. Seconds: {0}", gpNeededSeconds);

					// no need to wait for cordial, we will have GP faster
					return await WaitForGpRegain();
				}
			}

			if (gp + 300 >= AdjustedWaitForGp)
			{
				// If we used the cordial or the CordialType is only Cordial, not Auto or HiCordial, then return
				if (await UseCordial(CordialType.Cordial, ttg.RealSecondsTillStartGathering) || CordialType == CordialType.Cordial)
				{
					return await WaitForGpRegain();
				}
			}

			if (gp + 400 >= AdjustedWaitForGp)
			{
				if (await UseCordial(CordialType.HiCordial, ttg.RealSecondsTillStartGathering))
				{
					return await WaitForGpRegain();
				}
			}

			return await WaitForGpRegain();
		}

		private async Task<bool> CastTruth()
		{
			if (Me.CurrentJob != ClassJobType.Miner && Me.CurrentJob != ClassJobType.Botanist)
			{
				return false;
			}

			// TODO: Look into forcing casting this when flying under certain conditions.
			if (MovementManager.IsFlying || Me.ClassLevel < 46
				|| Me.HasAura(
					(int)(Me.CurrentJob == ClassJobType.Miner ? AbilityAura.TruthOfMountains : AbilityAura.TruthOfForests)))
			{
				return false;
			}

			while (Me.IsMounted && Behaviors.ShouldContinue)
			{
				await CommonTasks.StopAndDismount();
				await Coroutine.Yield();
			}

			return
				await
				CastAura(
					Ability.Truth,
					Me.CurrentJob == ClassJobType.Miner ? AbilityAura.TruthOfMountains : AbilityAura.TruthOfForests);
		}

		private bool ChangeHotSpot()
		{
			if (SpawnTimeout > 0 && DateTime.Now < startTime.AddSeconds(SpawnTimeout))
			{
				return false;
			}

			startTime = DateTime.Now;

			if (HotSpots != null)
			{
				// If finished current loop and set to not cyclic (we know this because if it was cyclic Next is always true)
				if (!HotSpots.Next())
				{
					Logger.Info("Finished {0} of {1} loops.", ++loopCount, Loops);

					// If finished all loops, otherwise just incrementing loop count
					if (loopCount == Loops)
					{
						isDone = true;
						return true;
					}

					// If not cyclic and it is on the last index
					if (!HotSpots.IsCyclic && HotSpots.Index == HotSpots.Count - 1)
					{
						HotSpots.Index = 0;
					}
				}
			}

			return true;
		}

		private void CheckForGatherRotationOverride()
		{
			if (!gatherRotation.CanBeOverriden || DisableRotationOverride)
			{
				if (!GatherItem.IsUnknown)
				{
					return;
				}

				Logger.Info("Item to gather is unknown, we are overriding the rotation to ensure we can collect it.");
			}

			var rotationAndTypes =
				Rotations.Select(r => new { Rotation = r.Value, OverrideValue = r.Value.ResolveOverridePriority(this) })
					.Where(r => r.OverrideValue > -1)
					.OrderByDescending(r => r.OverrideValue)
					.ToArray();

			var rotation = rotationAndTypes.FirstOrDefault();

			if (rotation == null || ReferenceEquals(rotation.Rotation, gatherRotation))
			{
				return;
			}

			Logger.Info(
				"Rotation Override -> Old: {0} , New: {1}",
				gatherRotation.Attributes.Name,
				rotation.Rotation.Attributes.Name);

			gatherRotation = rotation.Rotation;
		}

		private async Task<bool> ExecutePoiLogic()
		{
			if (Poi.Current.Type != PoiType.Gather)
			{
				return false;
			}

			var result = FindGatherSpot() || await GatherSequence();

			if (!result)
			{
				Poi.Clear("Something happened during gathering and we did not complete the sequence");
			}

			if (Poi.Current.Type == PoiType.Gather && (!Poi.Current.Unit.IsValid || !Poi.Current.Unit.IsVisible))
			{
				Poi.Clear("Node is gone");
			}

			return result;
		}

		private bool FindGatherSpot()
		{
			if (GatherSpot != null)
			{
				return false;
			}

			if (GatherSpots != null && Node.Location.Distance3D(Me.Location) > Distance)
			{
				GatherSpot =
					GatherSpots.OrderBy(gs => gs.NodeLocation.Distance3D(Node.Location))
						.FirstOrDefault(gs => gs.NodeLocation.Distance3D(Node.Location) <= Distance);
			}

			// Either GatherSpots is null, the node is already in range, or there are no matches, use fallback
			if (GatherSpot == null)
			{
				SetFallbackGatherSpot(Node.Location, true);
			}

			Logger.Info("GatherSpot set -> " + GatherSpot);

			return true;
		}

		private async Task<bool> FindNode(bool retryCenterHotspot = true)
		{
			if (Node != null)
			{
				return false;
			}

			StatusText = "Searching for nodes";

			while (Behaviors.ShouldContinue)
			{
				IEnumerable<GatheringPointObject> nodes =
					GameObjectManager.GetObjectsOfType<GatheringPointObject>().Where(gpo => gpo.CanGather).ToArray();

				if (GatherStrategy == GatherStrategy.TouchAndGo && HotSpots != null)
				{
					if (GatherObjects != null)
					{
						nodes = nodes.Where(gpo => GatherObjects.Contains(gpo.EnglishName, StringComparer.InvariantCultureIgnoreCase));
					}

					foreach (var node in
						nodes.Where(gpo => HotSpots.CurrentOrDefault.WithinHotSpot2D(gpo.Location))
							.OrderBy(gpo => gpo.Location.Distance2D(Me.Location))
							.Skip(1))
					{
						if (!Blacklist.Contains(node.ObjectId, BlacklistFlags.Interact))
						{
							Blacklist.Add(
								node,
								BlacklistFlags.Interact,
								TimeSpan.FromSeconds(18),
								"Skip furthest nodes in hotspot. We only want 1.");
						}
					}
				}

				nodes = nodes.Where(gpo => !Blacklist.Contains(gpo.ObjectId, BlacklistFlags.Interact));

				if (FreeRange)
				{
					nodes = nodes.Where(gpo => gpo.Distance2D(Me.Location) < Radius);
				}
				else
				{
					if (HotSpots != null)
					{
						nodes = nodes.Where(gpo => HotSpots.CurrentOrDefault.WithinHotSpot2D(gpo.Location));
					}
				}

				// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
				if (GatherObjects != null)
				{
					Node =
						nodes.OrderBy(
							gpo =>
							GatherObjects.FindIndex(i => string.Equals(gpo.EnglishName, i, StringComparison.InvariantCultureIgnoreCase)))
							.ThenBy(gpo => gpo.Location.Distance2D(Me.Location))
							.FirstOrDefault(gpo => GatherObjects.Contains(gpo.EnglishName, StringComparer.InvariantCultureIgnoreCase));
				}
				else
				{
					Node = nodes.OrderBy(gpo => gpo.Location.Distance2D(Me.Location)).FirstOrDefault();
				}

				if (Node == null)
				{
					if (HotSpots != null)
					{
						var myLocation = Me.Location;

						var distanceToFurthestVisibleGameObject =
							GameObjectManager.GameObjects.Select(o => o.Location.Distance2D(myLocation))
								.OrderByDescending(o => o)
								.FirstOrDefault();

						var distanceToFurthestVectorInHotspot = myLocation.Distance2D(HotSpots.CurrentOrDefault)
																+ HotSpots.CurrentOrDefault.Radius;

						if (myLocation.Distance2D(HotSpots.CurrentOrDefault) > Radius && GatherStrategy == GatherStrategy.GatherOrCollect && retryCenterHotspot
							&& distanceToFurthestVisibleGameObject <= distanceToFurthestVectorInHotspot)
						{
							Logger.Verbose("Distance to furthest visible game object -> " + distanceToFurthestVisibleGameObject);
							Logger.Verbose("Distance to furthest vector in hotspot -> " + distanceToFurthestVectorInHotspot);

							Logger.Warn(
								"Could not find any nodes and can not confirm hotspot is empty via object detection, trying again from center of hotspot.");
							await HotSpots.CurrentOrDefault.XYZ.MoveTo(radius: Radius, name: HotSpots.CurrentOrDefault.Name);

							retryCenterHotspot = false;
							await Coroutine.Yield();
							continue;
						}

						if (!ChangeHotSpot())
						{
							retryCenterHotspot = false;
							await Coroutine.Yield();
							continue;
						}
					}

					if (FreeRange && !FreeRangeConditional())
					{
						await Coroutine.Yield();
						isDone = true;
						return true;
					}

					return true;
				}

				var entry = Blacklist.GetEntry(Node.ObjectId);
				if (entry != null && entry.Flags.HasFlag(BlacklistFlags.Interact))
				{
					Logger.Warn("Node on blacklist, waiting until we move out of range or it clears.");

					if (await Coroutine.Wait(entry.Length, () => entry.IsFinished || Node.Location.Distance2D(Me.Location) > Radius))
					{
						if (!entry.IsFinished)
						{
							Node = null;
							Logger.Info("Node Reset, Reason: Ran out of range");
							return false;
						}
					}

					Logger.Info("Node removed from blacklist.");
				}

				Logger.Info("Node set -> " + Node);

				if (HotSpots == null)
				{
					MovementManager.SetFacing2D(Node.Location);
				}

				if (Poi.Current.Unit != Node)
				{
					Poi.Current = new Poi(Node, PoiType.Gather);
				}

				return true;
			}

			return true;
		}

		private bool FreeRangeConditional()
		{
			if (freeRangeConditionFunc == null)
			{
				freeRangeConditionFunc = ScriptManager.GetCondition(FreeRangeCondition);
			}

			return freeRangeConditionFunc();
		}

		private async Task<bool> Gather()
		{
			return await InteractWithNode() && await gatherRotation.Prepare(this) && await gatherRotation.ExecuteRotation(this)
					&& await gatherRotation.Gather(this) && await Coroutine.Wait(4000, () => !Node.CanGather)
					&& await WaitForGatherWindowToClose();
		}

		private async Task<bool> GatherSequence()
		{
			return await MoveToGatherSpot() && await BeforeGather() && await Gather() && await AfterGather()
					&& await MoveFromGatherSpot();
		}

		private static Type[] GetKnownRotationTypes()
		{
			return new[]
						{
							typeof(RegularNodeGatheringRotation), typeof(UnspoiledGatheringRotation),
							typeof(DefaultCollectGatheringRotation), typeof(Collect115GatheringRotation), typeof(Collect345GatheringRotation),
							typeof(Collect450GatheringRotation), typeof(Collect470GatheringRotation), typeof(Collect550GatheringRotation),
							typeof(Collect570GatheringRotation), typeof(DiscoverUnknownsGatheringRotation), typeof(ElementalGatheringRotation),
							typeof(TopsoilGatheringRotation), typeof(MapGatheringRotation), typeof(SmartQualityGatheringRotation),
							typeof(SmartYieldGatheringRotation), typeof(YieldAndQualityGatheringRotation),
							typeof(NewbCollectGatheringRotation)
						};
		}

		private TimeToGather GetTimeToGather()
		{
			var eorzeaMinutesTillDespawn = (int)byte.MaxValue;
			if (IsUnspoiled())
			{
				if (WorldManager.ZoneId > 350)
				{
					eorzeaMinutesTillDespawn = 55 - WorldManager.EorzaTime.Minute;
				}
				else
				{
					// We really don't know how much time is left on the node, but it does have at least the 5 more EM.
					eorzeaMinutesTillDespawn = 60 - WorldManager.EorzaTime.Minute;
				}
			}

			if (IsEphemeral())
			{
				var hoursFromNow = WorldManager.EorzaTime.AddHours(4);
				var rounded = new DateTime(
					hoursFromNow.Year,
					hoursFromNow.Month,
					hoursFromNow.Day,
					hoursFromNow.Hour - (hoursFromNow.Hour % 4),
					0,
					0);

				eorzeaMinutesTillDespawn = (int)(rounded - WorldManager.EorzaTime).TotalMinutes;
			}

			var realSecondsTillDespawn = eorzeaMinutesTillDespawn * 35 / 12;
			var realSecondsTillStartGathering = realSecondsTillDespawn - gatherRotation.Attributes.RequiredTimeInSeconds;

			return new TimeToGather
						{
							EorzeaMinutesTillDespawn = eorzeaMinutesTillDespawn,
							RealSecondsTillStartGathering = realSecondsTillStartGathering
						};
		}

		private bool HandleCondition()
		{
			if (WhileFunc == null)
			{
				WhileFunc = ScriptManager.GetCondition(While);
			}

			// If statement is true, return false so we can continue the routine
			if (WhileFunc())
			{
				return false;
			}

			isDone = true;
			return true;
		}

		private bool HandleDeath()
		{
			if (Me.IsDead && Poi.Current.Type != PoiType.Death)
			{
				Poi.Current = new Poi(Me, PoiType.Death);
				return true;
			}

			return false;
		}

		private bool HandleReset()
		{
			if (Node == null || (Node.IsValid && (!FreeRange || !(Node.Location.Distance3D(Me.Location) > Radius))))
			{
				return false;
			}

			OnResetCachedDone();
			return true;
		}

		private async Task<bool> InteractWithNode()
		{
			StatusText = "Interacting with node";

			var attempts = 0;
			while (attempts++ < 5 && !GatheringManager.WindowOpen && Behaviors.ShouldContinue && Poi.Current.Unit.IsVisible
					&& Poi.Current.Unit.IsValid)
			{
				var ticks = 0;
				while (MovementManager.IsFlying && ticks++ < 5 && Behaviors.ShouldContinue && Poi.Current.Unit.IsVisible
						&& Poi.Current.Unit.IsValid)
				{
					var ground = Me.Location.GetFloor(6);
					if (Math.Abs(ground.Y - Me.Location.Y) < float.Epsilon)
					{
						var mover = Navigator.PlayerMover as IFlightEnabledPlayerMover;
						if (mover != null && !mover.IsLanding && !mover.IsTakingOff)
						{
							await CommonTasks.DescendTo(ground.Y);
						}
					}

					await Coroutine.Sleep(200);
				}

				Poi.Current.Unit.Interact();

				if (await Coroutine.Wait(WindowDelay, () => GatheringManager.WindowOpen))
				{
					break;
				}

				if (attempts == 1 && WindowDelay <= 2000 && await Coroutine.Wait(WindowDelay, () => GatheringManager.WindowOpen))
				{
					// wait double on first attempt if delay less than 2 seconds.
					break;
				}

				if (FreeRange)
				{
					Logger.Warn("Gathering Window didn't open: Retrying. {0}/5", attempts);
					continue;
				}

				Logger.Warn("Gathering Window didn't open: Re-attempting to move into place. {0}/5", attempts);
				//SetFallbackGatherSpot(Node.Location, true);

				await MoveToGatherSpot();
			}

			if (!GatheringManager.WindowOpen)
			{
				if (!FreeRange)
				{
					await MoveFromGatherSpot();
				}

				OnResetCachedDone();
				return false;
			}

			interactedWithNode = true;

			Logger.Verbose("Started gathering from {0} with {1} GP at {2} ET", Node.EnglishName, Me.CurrentGP, WorldManager.EorzaTime.ToShortTimeString());

			if (!IsUnspoiled() && !IsConcealed())
			{
				if (!Blacklist.Contains(Poi.Current.Unit, BlacklistFlags.Interact))
				{
					var timeToBlacklist = GatherStrategy == GatherStrategy.TouchAndGo
											? TimeSpan.FromSeconds(15)
											: TimeSpan.FromSeconds(Math.Max(gatherRotation.Attributes.RequiredTimeInSeconds + 6, 30));
					Blacklist.Add(
						Poi.Current.Unit,
						BlacklistFlags.Interact,
						timeToBlacklist,
						"Blacklisting node so that we don't retarget -> " + Poi.Current.Unit);
				}
			}

			if (!await ResolveGatherItem())
			{
				await CloseGatheringWindow();
				ResetInternal();
				return false;
			}

			CheckForGatherRotationOverride();

			return true;
		}

		private int InventoryItemCount()
		{
			return InventoryManager.FilledSlots.Count(c => c.BagId != InventoryBagId.KeyItems);
		}

		private Dictionary<string, IGatheringRotation> LoadRotationTypes()
		{
			Type[] types = null;
			try
			{
				types =
					Assembly.GetExecutingAssembly()
						.GetTypes()
						.Where(
							t =>
							!t.IsAbstract && typeof(IGatheringRotation).IsAssignableFrom(t)
							&& t.GetCustomAttribute<GatheringRotationAttribute>() != null)
						.ToArray();
			}
			catch
			{
				Logger.Warn("Unable to get types, Loading Known Rotations.");
			}

			if (types == null)
			{
				types = GetKnownRotationTypes();
			}

			ReflectionHelper.CustomAttributes<GatheringRotationAttribute>.RegisterTypes(types);

			var instances = types.Select(t => t.CreateInstance<IGatheringRotation>()).ToArray();

			foreach (var instance in instances)
			{
				Logger.Info(
					"Loaded Rotation -> {0}, GP: {1}, Time: {2}",
					instance.Attributes.Name,
					instance.Attributes.RequiredGp,
					instance.Attributes.RequiredTimeInSeconds);
			}

			var dict = instances.ToDictionary(k => k.Attributes.Name, v => v, StringComparer.InvariantCultureIgnoreCase);

			return dict;
		}

		private async Task<bool> MoveFromGatherSpot()
		{
			return GatherSpot == null || await GatherSpot.MoveFromSpot(this);
		}

		private async Task<bool> MoveToGatherSpot()
		{
			var distance = Poi.Current.Location.Distance3D(Me.Location);
			if (FreeRange)
			{
				while (distance > Distance && distance <= Radius && Behaviors.ShouldContinue)
				{
					await Coroutine.Yield();
					distance = Poi.Current.Location.Distance3D(Me.Location);
				}
			}

			return distance <= Distance || await GatherSpot.MoveToSpot(this);
		}

		private async Task<bool> MoveToHotSpot()
		{
			if (HotSpots != null && !HotSpots.CurrentOrDefault.WithinHotSpot2D(Me.Location))
			{
				StatusText = "Moving to hotspot at " + HotSpots.CurrentOrDefault;

				await
					HotSpots.CurrentOrDefault.XYZ.MoveTo(radius: HotSpots.CurrentOrDefault.Radius * 0.75f,
						name: HotSpots.CurrentOrDefault.Name,
						stopCallback: MovementStopCallback);

				startTime = DateTime.Now;
				return true;
			}

			return false;
		}

		private async Task<bool> ResetOrDone()
		{
			while (Me.InCombat && Behaviors.ShouldContinue)
			{
				await Coroutine.Yield();
			}

			if (!FreeRange && (HotSpots == null || HotSpots.Count == 0 || (Node != null && IsUnspoiled() && interactedWithNode)))
			{
				isDone = true;
			}
			else
			{
				ResetInternal();
			}

			return true;
		}

		private void ResolveGatherRotation()
		{
			if (gatherRotation != null)
			{
				return;
			}

			if (GatheringSkillOrder != null && GatheringSkillOrder.GatheringSkills.Count > 0)
			{
				initialGatherRotation = gatherRotation = new GatheringSkillOrderGatheringRotation();

				Logger.Info("Using rotation -> " + gatherRotation.Attributes.Name);
				return;
			}

			IGatheringRotation rotation;
			if (!Rotations.TryGetValue(GatherRotation, out rotation))
			{
				// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
				if (!Rotations.TryGetValue("RegularNode", out rotation))
				{
					rotation = new RegularNodeGatheringRotation();
				}
				else
				{
					rotation = Rotations["RegularNode"];
				}

				Logger.Warn("Could not find rotation, using RegularNode instead.");
			}

			initialGatherRotation = gatherRotation = rotation;

			Logger.Info("Using rotation -> " + rotation.Attributes.Name);
		}

		private void SetFallbackGatherSpot(Vector3 location, bool useMesh)
		{
			switch (DefaultGatherSpotType)
			{
				// TODO: Smart stealth implementation (where any enemy within x distance and i'm not behind them, use stealth approach and set stealth location as current)
				// If flying, land in area closest to node not in sight of an enemy and stealth.
				case GatherSpotType.StealthGatherSpot:
					GatherSpot = new StealthGatherSpot { NodeLocation = location, UseMesh = useMesh };
					break;
				// ReSharper disable once RedundantCaseLabel
				case GatherSpotType.GatherSpot:
				default:
					GatherSpot = new GatherSpot { NodeLocation = location, UseMesh = useMesh };
					break;
			}
		}

		private bool SetGatherItemByItemName(ICollection<GatheringItem> windowItems)
		{
			foreach (var itemName in ItemNames)
			{
				GatherItem =
					windowItems.FirstOrDefault(
						i =>
						i.IsFilled && !i.IsUnknown
						&& string.Equals(itemName, i.ItemData.EnglishName, StringComparison.InvariantCultureIgnoreCase));

				if (GatherItem != null && (!GatherItem.ItemData.Unique || GatherItem.ItemData.ItemCount() == 0))
				{
					return true;
				}
			}

			return false;
		}

		private async Task<bool> UseCordial(CordialType cordialType, int maxTimeoutSeconds = 5)
		{
			maxTimeoutSeconds -= 2;
			if (CordialSpellData.Cooldown.TotalSeconds < maxTimeoutSeconds)
			{
				var cordial = InventoryManager.FilledSlots.FirstOrDefault(slot => slot.RawItemId == (uint)cordialType);

				if (cordial != null)
				{
					StatusText = "Using cordial when it becomes available";

					Logger.Info(
						"Using Cordial -> Waiting (sec): {0}, CurrentGP: {1}",
						(int)CordialSpellData.Cooldown.TotalSeconds,
						Me.CurrentGP);

					if (await Coroutine.Wait(
						TimeSpan.FromSeconds(maxTimeoutSeconds),
						() =>
							{
								if (Me.IsMounted && CordialSpellData.Cooldown.TotalSeconds < 2)
								{
									Actionmanager.Dismount();
									return false;
								}

								return cordial.CanUse(Me);
							}))
					{
						await Coroutine.Sleep(500);
						Logger.Info("Using " + cordialType);
						cordial.UseItem(Me);
						await Coroutine.Sleep(1500);
						return true;
					}
				}
				else
				{
					Logger.Warn("No Cordial avilable, buy more " + cordialType);
				}
			}

			return false;
		}

		private async Task<bool> WaitForGatherWindowToClose()
		{
			var ticks = 0;
			while (GatheringManager.WindowOpen && ticks++ < 100 && Behaviors.ShouldContinue)
			{
				await Coroutine.Yield();
			}

			return true;
		}

		private async Task<bool> WaitForGpRegain()
		{
			if (gatherRotation.ShouldForceGather)
			{
				return true;
			}

			var ttg = GetTimeToGather();

			if (Me.CurrentGP < AdjustedWaitForGp)
			{
				var gpNeeded = AdjustedWaitForGp - (Me.CurrentGP - (Me.CurrentGP % 5));
				var gpNeededTicks = gpNeeded / 5;
				var gpNeededSeconds = gpNeededTicks * 3;

				StatusText = "Waiting for GP";

				Logger.Info(
					"Waiting for GP -> Seconds: {0}, Current GP: {1}, WaitForGP: {2}",
					gpNeededSeconds,
					Me.CurrentGP,
					AdjustedWaitForGp);

				await
					Coroutine.Wait(
						TimeSpan.FromSeconds(ttg.RealSecondsTillStartGathering),
						() => Me.CurrentGP >= AdjustedWaitForGp || Me.CurrentGP == Me.MaxGP);
			}

			return true;
		}

		private struct TimeToGather
		{
#pragma warning disable 414
			public int EorzeaMinutesTillDespawn;
#pragma warning restore 414
			public int RealSecondsTillStartGathering;

			public int TicksTillStartGathering
			{
				get
				{
					return RealSecondsTillStartGathering / 3;
				}
			}
		}
	}
}