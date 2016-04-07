namespace ExBuddy
{
	using System.Threading.Tasks;
	using Buddy.Coroutines;
	using Clio.Utilities;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ff14bot;
	using ff14bot.Behavior;
	using ff14bot.Managers;
	using ff14bot.RemoteWindows;

	public class NoAetheryteUseTransportReturnStrategy : IReturnStrategy
	{
		public NoAetheryteUseTransportReturnStrategy()
		{
			DialogOption = -1;
			InteractDistance = 4.0f;
		}

		public int DialogOption { get; set; }

		public float InteractDistance { get; set; }

		public uint NpcId { get; set; }

		public Vector3 NpcLocation { get; set; }

		#region IAetheryteId Members

		public uint AetheryteId { get; set; }

		#endregion

		#region IZoneId Members

		public ushort ZoneId { get; set; }

		#endregion

		public override string ToString()
		{
			return string.Format(
				"NoAetheryteUseTransport: Death Location: {0}, AetheryteId: {1}, NpcLocation: {2}",
				InitialLocation,
				AetheryteId,
				NpcLocation);
		}

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

			await NpcLocation.MoveTo(true, radius: InteractDistance);
			GameObjectManager.GetObjectByNPCId(NpcId).Target();
			Core.Player.CurrentTarget.Interact();

			// Temporarily assume selectyesno until we see if we need it for anything but hinterlands
			await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
			SelectYesno.ClickYes();

			await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading);
			await CommonTasks.HandleLoading();

			await Coroutine.Sleep(2000);

			return true;
		}

		#endregion
	}
}