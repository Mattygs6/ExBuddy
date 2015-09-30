namespace ExBuddy.GameObjects.Npcs
{
	using Clio.Utilities;

	using ExBuddy.Interfaces;

	public class MasterPieceSupply : INpc
	{
		public uint AetheryteId { get; set; }

		public Vector3 Location { get; set; }

		public uint NpcId { get; set; }

		public ushort ZoneId { get; set; }

		public string Name { get; set; }
	}
}
