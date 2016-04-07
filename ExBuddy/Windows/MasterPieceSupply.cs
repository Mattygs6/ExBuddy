namespace ExBuddy.Windows
{
	using System;
	using System.Threading.Tasks;
	using Buddy.Coroutines;
	using ExBuddy.Enumerations;
	using ExBuddy.Helpers;
	using ExBuddy.Logging;
	using ff14bot.Enums;
	using ff14bot.Managers;
	using ff14bot.RemoteWindows;

	public sealed class MasterPieceSupply : Window<MasterPieceSupply>
	{
		public MasterPieceSupply()
			: base("MasterPieceSupply") {}

		public static uint GetClassIndex(ClassJobType classJobType)
		{
			return (uint) classJobType - 8;
		}

		public SendActionResult SelectClass(ClassJobType classJobType)
		{
			return SelectClass(GetClassIndex(classJobType));
		}

		public SendActionResult SelectClass(uint index)
		{
			return TrySendAction(2, 1, 2, 1, index);
		}

		public SendActionResult TurnIn(uint index)
		{
			return TrySendAction(2, 0, 0, 1, index);
		}

		public async Task<bool> TurnInAndHandOver(uint index, BagSlot bagSlot, byte attempts = 20, ushort interval = 200)
		{
			var result = SendActionResult.None;
			var requestAttempts = 0;
			while (result != SendActionResult.Success && !Request.IsOpen && requestAttempts++ < attempts
			       && Behaviors.ShouldContinue)
			{
				result = TurnIn(index);
				if (result == SendActionResult.InjectionError)
				{
					await Behaviors.Sleep(interval);
				}

				await Behaviors.Wait(interval, () => Request.IsOpen);
			}

			if (requestAttempts > attempts)
			{
				return false;
			}

			await Behaviors.Sleep(interval);

			// Try waiting half of the overall set time, up to 3 seconds
			if (!Request.IsOpen)
			{
				if (!await Coroutine.Wait(Math.Min(3000, (interval*attempts)/2), () => Request.IsOpen))
				{
					Logger.Instance.Warn(
						Localization.Localization.MasterPieceSupply_CollectabilityValueNotEnough,
						bagSlot.Collectability,
						bagSlot.EnglishName);
					return false;
				}
			}

			if (Memory.Request.ItemId1 != bagSlot.RawItemId)
			{
				Request.Cancel();
				var item = DataManager.GetItem(Memory.Request.ItemId1);
				Logger.Instance.Warn(
					Localization.Localization.MasterPieceSupply_CannotTurnIn,
					bagSlot.EnglishName,
					item.EnglishName);
				return false;
			}

			requestAttempts = 0;
			while (Request.IsOpen && requestAttempts++ < attempts && Behaviors.ShouldContinue && bagSlot.Item != null)
			{
				bagSlot.Handover();

				await Behaviors.Wait(interval, () => Request.HandOverButtonClickable);

				Request.HandOver();

				await Behaviors.Wait(interval, () => !Request.IsOpen || SelectYesno.IsOpen);
			}

			if (SelectYesno.IsOpen)
			{
				Logger.Instance.Warn(Localization.Localization.MasterPieceSupply_FullScrips, bagSlot.EnglishName);
				return false;
			}

			return !Request.IsOpen;
		}
	}
}