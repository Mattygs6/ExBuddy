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
			: this(npc.AetheryteId, npc.ZoneId, npc.Location, npc.NpcId, npc.Name) { }

		public uint AetheryteId { get; set; }

		public ushort ZoneId { get; set; }

		public Vector3 Location { get; set; }

		public uint NpcId { get; set; }

		public string Name { get; set; }
	}
}
