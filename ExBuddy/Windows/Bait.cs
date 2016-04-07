namespace ExBuddy.Windows
{
	using System.Threading.Tasks;
	using ExBuddy.Enumerations;
	using ExBuddy.Helpers;
	using ff14bot.Managers;

	public sealed class Bait : Window<Bait>
	{
		public Bait()
			: base("Bait") {}

		public async Task<bool> SelectBait(
			uint baitId,
			ushort baitDelay = 200,
			ushort maxWait = 2000,
			bool closeWindow = true)
		{
			if (!IsValid)
			{
				Actionmanager.DoAction(288, GameObjectManager.LocalPlayer);
				await Refresh(maxWait);
				await Behaviors.Sleep(maxWait);
			}

			var result = SendActionResult.None;
			var attempts = 0;
			while ((result != SendActionResult.Success || FishingManager.SelectedBaitItemId != baitId) && attempts++ < 3
			       && Behaviors.ShouldContinue)
			{
				result = SetBait(baitId);
				if (result == SendActionResult.InjectionError)
				{
					await Behaviors.Sleep(500);
				}

				await Behaviors.Wait(maxWait, () => FishingManager.SelectedBaitItemId == baitId);
			}

			if (closeWindow)
			{
				await CloseInstanceGently(interval: 500);
			}

			return result > SendActionResult.InjectionError;
		}

		public SendActionResult SetBait(uint baitId)
		{
			return Control.TrySendAction(4, 0, 0, 0, 0, 0, 0, 1, baitId);
		}
	}
}