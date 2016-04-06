
#pragma warning disable 1998

namespace ExBuddy
{
	using System.Linq;
	using System.Threading.Tasks;
	using Buddy.Coroutines;
	using Clio.Utilities;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ExBuddy.Logging;
	using ff14bot.Behavior;
	using ff14bot.Managers;
	using ff14bot.RemoteWindows;

	public class NoAetheryteUseAethernetReturnStrategy : IReturnStrategy, IInteractWithNpc
	{
		public NoAetheryteUseAethernetReturnStrategy()
		{
			InteractDistance = 8.0f;
			AethernetText = Localization.Localization.NoAetheryteUseAethernetReturn_AethernetText;
		}

		public string AethernetText { get; set; }

		public float InteractDistance { get; set; }

		public uint Slot { get; set; }

		#region IAetheryteId Members

		public uint AetheryteId { get; set; }

		#endregion

		#region IZoneId Members

		public ushort ZoneId { get; set; }

		#endregion

		#region IInteractWithNpc Members

		public Vector3 Location { get; set; }

		public uint NpcId { get; set; }

		#endregion

		#region IReturnStrategy Members

		public Vector3 InitialLocation { get; set; }

		public async Task<bool> ReturnToLocation()
		{
			if (BotManager.Current.EnglishName != "Fate Bot")
			{
				return await InitialLocation.MoveTo();
			}

			await Coroutine.Sleep(1000);
			return true;
		}

		public async Task<bool> ReturnToZone()
		{
			await this.TeleportTo();

			await this.Interact();

			await Coroutine.Wait(5000, () => SelectString.IsOpen);
			if (!SelectString.IsOpen)
			{
				Logger.Instance.Error("Timeout, SelectString window did not open.");
				return false;
			}

			if (SelectString.Lines().Any(line => line.Contains(AethernetText)))
			{
				Logger.Instance.Info("Selecting line " + AethernetText);
				SelectString.ClickLineContains(AethernetText);
				// SelectString.ClickSlot(0);  going to try to make it more compatible with possible changes to game.

				await Coroutine.Wait(5000, () => !SelectString.IsOpen);
				await Coroutine.Wait(10000, () => SelectString.IsOpen);

				if (!SelectString.IsOpen)
				{
					Logger.Instance.Error("Timeout, SelectString window did not open.");
					return false;
				}
			}

			Logger.Instance.Info("Selecting line " + Slot);
			SelectString.ClickSlot(Slot);

			await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading);
			await CommonTasks.HandleLoading();

			await Coroutine.Sleep(2000); // Weird stuff happens without this.

			return true;
		}

		#endregion
	}
}