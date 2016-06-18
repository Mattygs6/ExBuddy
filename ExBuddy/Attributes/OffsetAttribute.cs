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

	/// <summary>
	///     Make it a different type so its easier to grab
	/// </summary>
	[AttributeUsage(AttributeTargets.All)]
	public class Offset64 : Attribute
	{
		public bool IsOffset;

		public int Modifier;

		public bool MultipleResults;

		public string Pattern;

		public Offset64(string pattern, bool isoffset = false, int modifier = 0, bool multresults = false)
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