﻿namespace ExBuddy
{
	using System.Threading.Tasks;
	using Buddy.Coroutines;
	using Clio.Utilities;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ff14bot.Managers;

	public class DefaultReturnStrategy : IReturnStrategy
	{
		#region IAetheryteId Members

		public uint AetheryteId { get; set; }

		#endregion

		#region IZoneId Members

		public ushort ZoneId { get; set; }

		#endregion

		public override string ToString()
		{
#if RB_CN
			return string.Format("默认: 死亡地点: {0}, 以太之光ID: {1}", InitialLocation, AetheryteId);
#else
            return string.Format("Default: Death Location: {0}, AetheryteId: {1}", InitialLocation, AetheryteId);
#endif
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

			return true;
		}

#endregion
	}
}