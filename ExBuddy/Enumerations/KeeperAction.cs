namespace ExBuddy.Enumerations
{
	using System;

	[Serializable]
	[Flags]
	public enum KeeperAction : byte
	{
		DontKeep = 0x00,

		KeepNq = 0x01,

		KeepHq = 0x02,
		// MoochFlag = 0x04,
		KeepAll = 0x03, // KeepNq | KeepHq

		Mooch = 0x06, // KeepHq | MoochFlag

		MoochKeepNq = 0x07 // KeepNq | KeepHq | MoochFlag
	}
}