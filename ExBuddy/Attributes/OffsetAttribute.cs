namespace ExBuddy.Attributes
{
	using System;

	[AttributeUsage(AttributeTargets.All)]
	public class Offset : Attribute
	{
		public bool IsOffset;

		public int Modifier;

		public bool MultipleResults;

		public string Pattern;

		public Offset(string pattern, bool isoffset = false, int modifier = 0, bool multresults = false)
		{
			Pattern = pattern;
			IsOffset = isoffset;
			Modifier = modifier;
			MultipleResults = multresults;
		}
	}

	[AttributeUsage(AttributeTargets.All)]
	public class OffsetCN : Offset
	{
		public OffsetCN(string pattern, bool isoffset = false, int modifier = 0, bool multresults = false)
			: base(pattern, isoffset, modifier, multresults) {}
	}
}