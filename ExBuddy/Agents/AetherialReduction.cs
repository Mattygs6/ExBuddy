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
				return Core.Memory.Read<IntPtr>(Pointer + 56);
			}
		}

		public uint Purity
		{
			get
			{
				return Core.Memory.Read<uint>(Pointer + 60);
			}
		}

		public uint MaxPurity
		{
			get
			{
				return Core.Memory.Read<uint>(Pointer + 64);
			}
		}
	}
}
