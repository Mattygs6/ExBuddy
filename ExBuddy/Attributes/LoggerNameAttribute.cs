namespace ExBuddy.Attributes
{
	using System;

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class LoggerNameAttribute : Attribute
	{
		public LoggerNameAttribute(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Cannot be null or whitespace.", "name");
			}

			Name = name;
		}

		public string Name { get; private set; }
	}
}