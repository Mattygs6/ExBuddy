namespace ExBuddy.Attributes
{
	using System;

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class GatheringRotationAttribute : Attribute
	{
		public GatheringRotationAttribute(string name)
			: this(name, 0, 0) {}

		public GatheringRotationAttribute(string name, byte requiredTimeInSeconds, params ushort[] requiredGpBreakpoints)
		{
			Name = name;
			RequiredGpBreakpoints = requiredGpBreakpoints != null && requiredGpBreakpoints.Length > 0
				? requiredGpBreakpoints
				: new ushort[] {0};
			RequiredTimeInSeconds = requiredTimeInSeconds;
		}

		public string Name { get; private set; }

		public ushort[] RequiredGpBreakpoints { get; set; }

		public byte RequiredTimeInSeconds { get; set; }
	}
}