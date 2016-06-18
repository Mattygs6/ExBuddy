
#pragma warning disable 1998

namespace ExBuddy
{
	using System.Threading.Tasks;
	using Clio.Utilities;
	using ExBuddy.Interfaces;

	public class NoAetheryteRunToZoneReturnStrategy : IReturnStrategy
	{
		public Vector3 ZoneLocation { get; set; }

		#region IAetheryteId Members

		public uint AetheryteId { get; set; }

		#endregion

		#region IZoneId Members

		public ushort ZoneId { get; set; }

		#endregion

		#region IReturnStrategy Members

		public Vector3 InitialLocation { get; set; }

		public async Task<bool> ReturnToLocation()
		{
			return true;
		}

		public async Task<bool> ReturnToZone()
		{
			return true;
		}

		#endregion
	}
}