namespace ExBuddy.Offsets
{
	using System;

	using ExBuddy.Attributes;

	public static class Bait
	{
		//dword_1442828
		[Offset("Search 3B 05 ? ? ? ? 74 D8 Add 2 Read32")]
		public static IntPtr SelectedBaitItemIdPointer;
	}
}