namespace ExBuddy.Enums
{
	using System;

	[Flags]
	public enum CordialTime : byte
	{
		None = 0,

		BeforeGather = 1 << 0,

		AfterGather = 1 << 1,

		IfNeeded = 1 << 2,

		IfNeededOrAfter = IfNeeded | AfterGather,

		Auto = BeforeGather | AfterGather
	}
}