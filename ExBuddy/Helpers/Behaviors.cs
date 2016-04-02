
#pragma warning disable 1998

namespace ExBuddy.Helpers
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using Buddy.Coroutines;
	using Clio.Utilities;
	using ExBuddy.Interfaces;
	using ExBuddy.Logging;
	using ExBuddy.Navigation;
	using ff14bot;
	using ff14bot.Behavior;
	using ff14bot.Enums;
	using ff14bot.Managers;
	using ff14bot.Navigation;
	using ff14bot.Settings;

	public static class Behaviors
	{
		public static readonly Func<float, float, bool> DontStopInRange = (d, r) => false;

		private static bool shouldContinue;

		static Behaviors()
		{
			Behaviors.ShouldContinue = true;
			TreeRoot.OnStart += bot => Behaviors.ShouldContinue = true;
			TreeRoot.OnStop += bot => Behaviors.ShouldContinue = false;
		}

		public static bool ShouldContinue
		{
			get { return Core.Player.IsAlive && shouldContinue; }

			internal set { shouldContinue = value; }
		}

		public static async Task<bool> Dismount(byte maxTicks = 100, ushort interval = 100)
		{
			var dismountTicks = 0;
			while (dismountTicks++ < maxTicks && Core.Player.IsMounted && Behaviors.ShouldContinue)
			{
				if (MovementManager.IsFlying)
				{
					if (Navigator.PlayerMover is FlightEnabledSlideMover)
					{
						Navigator.Stop();
					}
					else
					{
						MovementManager.StartDescending();
					}
				}
				else
				{
					Actionmanager.Dismount();
				}

				await Wait(interval, () => !Core.Player.IsMounted);
			}

			if (dismountTicks > maxTicks)
			{
				Logger.Instance.Error("Failed to dismount.");
				return false;
			}

			return true;
		}

		public static IReturnStrategy GetReturnStrategy()
		{
			var currentZoneId = WorldManager.ZoneId;
			var teleportLocation = WorldManager.AvailableLocations.FirstOrDefault(l => l.ZoneId == currentZoneId);

			if (teleportLocation.AetheryteId == 0)
			{
				return GetReturnStrategyForZoneWithoutAetheryte(currentZoneId);
			}

			return new DefaultReturnStrategy
			{
				ZoneId = currentZoneId,
				AetheryteId = teleportLocation.AetheryteId,
				InitialLocation = Core.Player.Location
			};
		}

		public static IReturnStrategy GetReturnStrategyForZoneWithoutAetheryte(ushort zoneId)
		{
			IReturnStrategy strategy;
			switch (zoneId)
			{
				case 399:
					////strategy = new NoAetheryteUseTransportReturnStrategy
					////				{
					////					InteractDistance = 3.0f,
					////					ZoneId = 478,
					////					AetheryteId = 75,
					////					InitialLocation = Core.Player.Location,
					////					NpcId = 1015570,
					////					NpcLocation = new Vector3(63.45142f, 207.29f, -2.773367f)
					////				};
					//// 3.1 change

					strategy = new NoAetheryteUseAethernetReturnStrategy
					{
						ZoneId = 478,
						AetheryteId = 75,
						NpcId = 75,
						InitialLocation = GameObjectManager.LocalPlayer.Location,
						Location = new Vector3(71.94617f, 211.2611f, -18.90594f),
						Slot = 2
					};
					break;
				default:
					strategy = new NoOpReturnStrategy();
					break;
			}

			return strategy;
		}

		public static async Task<bool> Mount(Vector3? destination = null, uint mountId = 0)
		{
			if (await ShouldMount(destination))
			{
				uint flightSpecificMountId = 0;
				if (mountId == 0)
				{
					var playerMover = Navigator.PlayerMover as IFlightEnabledPlayerMover;
					if (playerMover != null)
					{
						flightSpecificMountId = (uint) playerMover.FlightMovementArgs.MountId;
					}
				}

				var ticks = 0;
				while (!Core.Player.IsMounted && ticks++ < 10 && Behaviors.ShouldContinue)
				{
					if (WorldManager.CanFly && flightSpecificMountId > 0)
					{
						if (!await CommonTasks.MountUp(flightSpecificMountId))
						{
							await CommonTasks.MountUp();
						}

						await Coroutine.Yield();
						if (Core.Player.IsMounted)
						{
							break;
						}
					}

					if (mountId == 0 || !await CommonTasks.MountUp(mountId))
					{
						await CommonTasks.MountUp();
					}

					await Coroutine.Yield();
				}
			}

			return true;
		}

		public static async Task<bool> MoveTo(
			this HotSpot hotspot,
			bool useMesh = true,
			uint mountId = 0,
			Func<float, float, bool> stopCallback = null,
			bool dismountAtDestination = false)
		{
			return await MoveTo(hotspot.XYZ, useMesh, mountId, hotspot.Radius, hotspot.Name, stopCallback, dismountAtDestination);
		}

		public static async Task<bool> MoveTo(
			this Vector3 destination,
			bool useMesh = true,
			uint mountId = 0,
			float radius = 2.0f,
			string name = null,
			Func<float, float, bool> stopCallback = null,
			bool dismountAtDestination = false)
		{
			await Mount(destination, mountId);
			await MoveToNoMount(destination, useMesh, radius, name, stopCallback);
			return !dismountAtDestination || await Dismount();
		}

		public static async Task<bool> MoveToNoMount(
			this HotSpot hotspot,
			bool useMesh = true,
			Func<float, float, bool> stopCallback = null)
		{
			return await MoveToNoMount(hotspot.XYZ, useMesh, hotspot.Radius, hotspot.Name, stopCallback);
		}

		public static async Task<bool> MoveToNoMount(
			this Vector3 destination,
			bool useMesh = true,
			float radius = 2.0f,
			string name = null,
			Func<float, float, bool> stopCallback = null)
		{
			stopCallback = stopCallback ?? ((d, r) => d <= r);

			var sprintDistance = Math.Min(20.0f, CharacterSettings.Instance.MountDistance);
			float distance;
			if (useMesh)
			{
				var moveResult = MoveResult.GeneratingPath;
				while (Behaviors.ShouldContinue
				       && (!stopCallback(distance = Core.Player.Location.Distance3D(destination), radius)
				           || stopCallback == DontStopInRange) && !(moveResult.IsDoneMoving()))
				{
					moveResult = Navigator.MoveTo(destination, name);
					await Coroutine.Yield();

					if (distance > sprintDistance)
					{
						await Sprint();
					}
				}

				Navigator.Stop();
			}
			else
			{
				while (Behaviors.ShouldContinue && !stopCallback(distance = Core.Player.Location.Distance3D(destination), radius))
				{
					Navigator.PlayerMover.MoveTowards(destination);
					await Coroutine.Yield();

					if (distance > sprintDistance)
					{
						await Sprint();
					}
				}

				Navigator.PlayerMover.MoveStop();
			}

			return true;
		}

		public static async Task<bool> MoveToPointWithin(
			this HotSpot hotspot,
			uint mountId = 0,
			bool dismountAtDestination = false)
		{
			return await MoveToPointWithin(hotspot.XYZ, hotspot.Radius, mountId, hotspot.Name, dismountAtDestination);
		}

		public static async Task<bool> MoveToPointWithin(
			this Vector3 destination,
			float radius,
			uint mountId = 0,
			string name = null,
			bool dismountAtDestination = false)
		{
			await Mount(destination, mountId);
			await MoveToPointWithinNoMount(destination, radius, name);
			return !dismountAtDestination || await Dismount();
		}

		public static async Task<bool> MoveToPointWithinNoMount(this Vector3 destination, float radius, string name = null)
		{
			var sprintDistance = Math.Min(20.0f, CharacterSettings.Instance.MountDistance);

			var moveResult = MoveResult.GeneratingPath;
			while (Behaviors.ShouldContinue && !(moveResult.IsDoneMoving()))
			{
				moveResult = Navigator.MoveToPointWithin(destination, radius, name);
				await Coroutine.Yield();

				var distance = Core.Player.Location.Distance3D(destination);
				if (distance > sprintDistance)
				{
					await Sprint();
				}
			}

			Navigator.Stop();

			return true;
		}

		public static async Task<bool> ShouldMount(Vector3? destination = null)
		{
			if (Core.Player.IsMounted)
			{
				return false;
			}

			if (destination.HasValue)
			{
				if (destination.Value.IsGround())
				{
					if (!CharacterSettings.Instance.UseMount)
					{
						return false;
					}

					if (Core.Player.Location.Distance3D(destination.Value) < CharacterSettings.Instance.MountDistance)
					{
						return false;
					}
				}
			}

			return Actionmanager.CanMount == 0;
		}

		public static async Task Sleep(int interval)
		{
			if (interval <= 33)
			{
				await Coroutine.Yield();
			}
			else
			{
				await Coroutine.Sleep(interval);
			}
		}

		public static async Task<bool> Sprint(int timeout = 500)
		{
			if (Actionmanager.IsSprintReady && !Core.Player.IsCasting && !Core.Player.IsMounted && Core.Player.CurrentTP == 1000
			    && MovementManager.IsMoving)
			{
				Actionmanager.Sprint();

				// Maybe use MovementManager speed.
				await Coroutine.Wait(500, () => !Actionmanager.IsSprintReady);
			}

			return true;
		}

		public static async Task<bool> TeleportTo(ushort zoneId, uint aetheryteId)
		{
			if (WorldManager.ZoneId == zoneId)
			{
				// continue we are in the zone.
				return false;
			}

			var ticks = 0;
			while (MovementManager.IsMoving && ticks++ < 5 && Behaviors.ShouldContinue)
			{
				Navigator.Stop();
				await Coroutine.Sleep(240);
			}

			var casted = false;
			while (WorldManager.ZoneId != zoneId && Behaviors.ShouldContinue)
			{
				if (!Core.Player.IsCasting && casted)
				{
					break;
				}

				if (!Core.Player.IsCasting && !CommonBehaviors.IsLoading)
				{
					WorldManager.TeleportById(aetheryteId);
					await Coroutine.Sleep(500);
				}

				casted = casted || Core.Player.IsCasting;
				await Coroutine.Yield();
			}

			await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading);
			await Coroutine.Wait(10000, () => !CommonBehaviors.IsLoading);

			return true;
		}

		public static async Task<bool> TeleportTo(ushort zoneId)
		{
			var teleportLocation = WorldManager.AvailableLocations.FirstOrDefault(l => l.ZoneId == zoneId);

			if (teleportLocation.AetheryteId == 0)
			{
				return false;
			}

			return await TeleportTo(zoneId, teleportLocation.AetheryteId);
		}

		public static async Task<bool> TeleportTo(uint aetheryteId)
		{
			var zoneId = WorldManager.GetZoneForAetheryteId(aetheryteId);

			if (zoneId == 0)
			{
				return false;
			}

			return await TeleportTo((ushort) zoneId, aetheryteId);
		}

		public static async Task<bool> TeleportTo(this ITeleportLocation teleportLocation)
		{
			return await TeleportTo(teleportLocation.ZoneId, teleportLocation.AetheryteId);
		}

		public static async Task<bool> Unstuck()
		{
			await CommonTasks.DescendTo(0);
			return true;
		}

		public static async Task Wait(int interval, Func<bool> condition)
		{
			if (interval <= 33)
			{
				await Coroutine.Yield();
			}
			else
			{
				await Coroutine.Wait(interval, condition);
			}
		}
	}
}