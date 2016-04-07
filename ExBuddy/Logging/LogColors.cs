namespace ExBuddy.Logging
{
	using System.Windows.Media;
	using ExBuddy.Interfaces;

	public class LogColors : ILogColors
	{
		#region ILogColors Members

		public virtual Color Error
		{
			get { return Colors.Red; }
		}

		public virtual Color Info
		{
			get { return Colors.DarkKhaki; }
		}

		public virtual Color Warn
		{
			get { return Colors.PaleVioletRed; }
		}

		#endregion
	}
}