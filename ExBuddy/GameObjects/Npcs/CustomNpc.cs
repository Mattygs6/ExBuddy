namespace ExBuddy.GameObjects.Npcs
{
	using Clio.Utilities;
	using ExBuddy.Interfaces;

	public class CustomNpc : INpc
	{
		public CustomNpc(uint aetheryteId, ushort zoneId, Vector3 location, uint npcId, string name)
		{
			AetheryteId = aetheryteId;
			ZoneId = zoneId;
			Location = location;
			NpcId = npcId;
			Name = name;
		}

		public CustomNpc(INpc npc)
			: this(npc.AetheryteId, npc.ZoneId, npc.Location, npc.NpcId, npc.Name) {}

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