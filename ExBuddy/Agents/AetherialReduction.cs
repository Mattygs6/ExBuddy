namespace ExBuddy.Agents
{
	using System;
	using ExBuddy.Offsets;
	using ff14bot;

	public sealed class AetherialReduction : Agent<AetherialReduction>
	{
		public AetherialReduction()
			: base(172) {}

		public IntPtr CurrentBagSlot
		{
			get { return Core.Memory.Read<IntPtr>(Pointer + AetherialReductionOffsets.CurrentBagSlotOffset); }
		}

		public uint MaxPurity
		{
			get { return Core.Memory.Read<uint>(Pointer + AetherialReductionOffsets.MaxPurityOffset); }
		}

		public uint Purity
		{
			get { return Core.Memory.Read<uint>(Pointer + AetherialReductionOffsets.PurityOffset); }
		}
	}
}