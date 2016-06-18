namespace ExBuddy.Agents
{
	using System;
	using ExBuddy.Offsets;
	using ff14bot;

	public sealed class Desynthesis : Agent<Desynthesis>
	{
		public Desynthesis()
			: base(114) {}

		public IntPtr CurrentBagSlot
		{
			get { return Core.Memory.Read<IntPtr>(Pointer + DesynthesisOffsets.CurrentBagSlotOffset); }
		}
	}
}