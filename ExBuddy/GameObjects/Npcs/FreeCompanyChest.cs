﻿namespace ExBuddy.GameObjects.Npcs
{
	using Clio.Utilities;
	using ExBuddy.Interfaces;

	public class FreeCompanyChest : INpc
	{
		#region IAetheryteId Members

		public uint AetheryteId { get; set; }

		#endregion

		#region INamedItem Members

		public string Name { get; set; }

		#endregion

		#region IZoneId Members

		public ushort ZoneId { get; set; }

		#endregion

		#region IInteractWithNpc Members

		public Vector3 Location { get; set; }

		public uint NpcId { get; set; }

		#endregion
	}
}