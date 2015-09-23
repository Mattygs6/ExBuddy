namespace ExBuddy.Logging
{
	using ExBuddy.Interfaces;

	public class LogColors : ILogColors
	{
		public virtual System.Windows.Media.Color Error
		{
			get
			{
				return System.Windows.Media.Colors.Red;
			}
		}

		public virtual System.Windows.Media.Color Warn
		{
			get
			{
				return System.Windows.Media.Colors.PaleVioletRed;
			}
		}

		public virtual System.Windows.Media.Color Info
		{
			get
			{
				return System.Windows.Media.Colors.DarkKhaki;
			}
		}
	}
}