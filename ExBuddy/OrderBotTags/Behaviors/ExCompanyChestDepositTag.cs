namespace ExBuddy.OrderBotTags.Behaviors
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Media;
	using Buddy.Coroutines;
	using Clio.Utilities;
	using Clio.XmlEngine;
	using ExBuddy.Attributes;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ExBuddy.OrderBotTags.Behaviors.Objects;
	using ExBuddy.Windows;
	using ff14bot;
	using ff14bot.Enums;
	using ff14bot.Managers;
	using ff14bot.NeoProfiles;

	[LoggerName("ExCompanyChestDeposit")]
	[XmlElement("ExCompanyChestDeposit")]
	public sealed class ExCompanyChestDepositTag : ExProfileBehavior, INpc
	{
		private readonly Stopwatch interactTimeout = new Stopwatch();

		private INpc freeCompanyChestNpc;

		private uint[] ids;

		[DefaultValue(true)]
		[XmlAttribute("Consolidate")]
		public bool Consolidate { get; set; }

		[XmlAttribute("ItemIds")]
		public int[] ItemIds { get; set; }

		[DefaultValue(Locations.UldahStepsOfNald)]
		[XmlAttribute("Location")]
		public Locations Location { get; set; }

		[DefaultValue(60)]
		[XmlAttribute("Timeout")]
		public int Timeout { get; set; }

		protected override Color Info
		{
			get { return Colors.DarkGray; }
		}

		private uint[] Ids
		{
			get { return ids ?? (ids = ItemIds.Select(Convert.ToUInt32).ToArray()); }
		}

		protected override void DoReset()
		{
			interactTimeout.Reset();
		}

		protected override async Task<bool> Main()
		{
			if (interactTimeout.Elapsed.TotalSeconds > Timeout)
			{
				Logger.Error(Localization.Localization.ExCompanyChestDeposit_Timeout);
				isDone = true;
				return true;
			}

			if (await freeCompanyChestNpc.TeleportTo())
			{
				return true;
			}

			// Movement
			if (ExProfileBehavior.Me.Distance(freeCompanyChestNpc.Location) > 3.5)
			{
				StatusText = "Moving to Npc -> " + freeCompanyChestNpc.NpcId;

				await freeCompanyChestNpc.Location.MoveTo(radius: 3.4f, name: " NpcId: " + freeCompanyChestNpc.NpcId);
				return true;
			}

			if (!interactTimeout.IsRunning)
			{
				interactTimeout.Restart();
			}

			var freeCompanyChest = new FreeCompanyChest();

			// Interact
			if (Core.Target == null && ExProfileBehavior.Me.Distance(freeCompanyChestNpc.Location) <= 3.5)
			{
				await freeCompanyChestNpc.Interact();
				await freeCompanyChest.Refresh(2000);
				return true;
			}

			if (freeCompanyChest.IsValid)
			{
				if (interactTimeout.Elapsed.TotalSeconds > Timeout)
				{
					await freeCompanyChest.CloseInstance();
					return true;
				}

				if (Consolidate && ConditionParser.FreeItemSlots() > 0)
				{
					StatusText = "Consolidating...";
					Logger.Info("Consolidating...");

					var chestBagSlots =
						new List<BagSlot>(
							InventoryManager.GetBagsByInventoryBagId(
								InventoryBagId.GrandCompany_Page1,
								InventoryBagId.GrandCompany_Page2,
								InventoryBagId.GrandCompany_Page3).SelectMany(bag => bag.Select(bagSlot => bagSlot)));

					var bagGroups =
						chestBagSlots.Where(bs => bs.IsFilled)
							.GroupBy(bs => bs.TrueItemId)
							.Select(
								g => new {g.Key, BagSlots = g.Where(bs => !bs.IsFullStack(true)).OrderByDescending(bs => bs.Count).ToList()})
							.ToArray();

					foreach (var bagGroup in bagGroups.Where(g => g.BagSlots.Count > 1))
					{
						Logger.Info("Found item to consolidate -> Id: {0}, BagSlots: {1}", bagGroup.Key, bagGroup.BagSlots.Count);

						for (var i = 0; i < bagGroup.BagSlots.Count; i++)
						{
							var destinationBagSlot = bagGroup.BagSlots[i];
							for (var j = i + 1; j < bagGroup.BagSlots.Count; j++)
							{
								if (destinationBagSlot.IsFullStack())
								{
									break;
								}

								var sourceBagSlot = bagGroup.BagSlots[j];

								if (!sourceBagSlot.IsFilled)
								{
									continue;
								}

								if (sourceBagSlot.BagId == destinationBagSlot.BagId)
								{
									MoveItem(sourceBagSlot, destinationBagSlot);
								}
								else
								{
									var tempBagSlot =
										InventoryManager.GetBagsByInventoryBagId(
											InventoryBagId.Bag1,
											InventoryBagId.Bag2,
											InventoryBagId.Bag3,
											InventoryBagId.Bag4)
											.Select(bag => bag.FirstOrDefault(bagSlot => !bagSlot.IsFilled))
											.FirstOrDefault(bagSlot => bagSlot != null);

									if (tempBagSlot == null)
									{
										Logger.Error("We somehow have a full inventory and cannot consolidate");
										return isDone = true;
									}

									MoveItem(sourceBagSlot, tempBagSlot);

									await Coroutine.Sleep(1500);

									MoveItem(tempBagSlot, destinationBagSlot);
								}

								await Coroutine.Sleep(1000);
							}
						}
					}
				}

				// Now move items
				foreach (var itemId in Ids)
				{
					// TODO: Might need unique check, but most likely not, spiritbond should take care of collectable
					var myBagSlots =
						InventoryManager.FilledInventoryAndArmory.Where(
							bs => itemId == bs.RawItemId && bs.SpiritBond < float.Epsilon && !bs.Item.Untradeable)
							.GroupBy(bs => bs.TrueItemId)
							.Select(g => new {g.Key, BagSlots = g.OrderBy(bs => bs.Count).ToList()})
							.ToArray();

					var chestSlots =
						new List<BagSlot>(
							InventoryManager.GetBagsByInventoryBagId(
								InventoryBagId.GrandCompany_Page1,
								InventoryBagId.GrandCompany_Page2,
								InventoryBagId.GrandCompany_Page3)
								.SelectMany(
									bag => bag.Where(bagSlot => !bagSlot.IsFilled || (itemId == bagSlot.RawItemId && !bagSlot.IsFullStack(true))))
								.OrderByDescending(bs => bs.Count));

					var groups = chestSlots.GroupBy(bs => bs.TrueItemId).Select(g => new {g.Key, BagSlots = g.ToArray()}).ToArray();

					foreach (var sourceBags in myBagSlots)
					{
						var destBags =
							groups.Where(g => g.Key == sourceBags.Key || g.Key == 0)
								.SelectMany(g => g.BagSlots)
								.OrderByDescending(bs => bs.Count)
								.ToList();

						foreach (var destinationBagSlot in destBags)
						{
							foreach (var sourceBagSlot in sourceBags.BagSlots)
							{
								if (destinationBagSlot.IsFullStack())
								{
									break;
								}

								if (!sourceBagSlot.IsFilled)
								{
									continue;
								}

								MoveItem(sourceBagSlot, destinationBagSlot);

								await Coroutine.Sleep(1000);
							}
						}
					}

					await Coroutine.Sleep(1000);
				}

				await freeCompanyChest.CloseInstanceGently();

				return isDone = true;
			}

			return true;
		}

		protected override void OnDone()
		{
			interactTimeout.Stop();

			if (Window<FreeCompanyChest>.IsOpen)
			{
				FreeCompanyChest.Close();
			}
		}

		protected override void OnStart()
		{
			if (Location == Locations.Custom)
			{
				freeCompanyChestNpc = this;
			}
			else
			{
				freeCompanyChestNpc = Data.GetNpcsByLocation<GameObjects.Npcs.FreeCompanyChest>(Location).FirstOrDefault();
			}
		}

		private void MoveItem(BagSlot source, BagSlot destination)
		{
			Logger.Verbose(
				"Moving {0} {1} from [{2},{3}] to [{4},{5}]",
				Math.Min(99 - destination.Count, source.Count),
				source.IsHighQuality ? source.EnglishName + " HQ" : source.EnglishName,
				(int) source.BagId,
				source.Slot,
				(int) destination.BagId,
				destination.Slot);

			source.Move(destination);
		}

		#region CustomLocationInfo

		[XmlAttribute("NpcId")]
		public uint NpcId { get; set; }

		[XmlAttribute("AetheryteId")]
		public uint AetheryteId { get; set; }

		[XmlAttribute("ZoneId")]
		public ushort ZoneId { get; set; }

		[XmlAttribute("NpcLocation")]
		Vector3 IInteractWithNpc.Location { get; set; }

		#endregion
	}
}