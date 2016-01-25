using ExBuddy.Offsets;

namespace ExBuddy.Agents
{
	using System;

	using ff14bot;

	public sealed class AetherialReduction : Agent<AetherialReduction>
	{
		public AetherialReduction()
			: base(172) { }

		public IntPtr CurrentBagSlot
		{
			get
			{
				return Core.Memory.Read<IntPtr>(Pointer + AetherialReductionOffsets.CurrentBagSlotOffset);
			}
		}

		public uint Purity
		{
			get
			{
				return Core.Memory.Read<uint>(Pointer + AetherialReductionOffsets.PurityOffset);
			}
		}

		public uint MaxPurity
		{
			get
			{
				return Core.Memory.Read<uint>(Pointer + AetherialReductionOffsets.MaxPurityOffset);
			}
		}
	}
}
