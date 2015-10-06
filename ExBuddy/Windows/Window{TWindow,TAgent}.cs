namespace ExBuddy.Windows
{
	using ExBuddy.Agents;

	public abstract class Window<TWindow, TAgent> : Window<TWindow>
		where TWindow : Window<TWindow>, new()
		where TAgent : Agent<TAgent>, new()
	{
		protected static TAgent Agent = new TAgent();

		protected Window(string name) : base(name) { } 

		public static void Open()
		{
			if (!IsOpen)
			{
				Agent.ToggleInstance();
			}
		}

		public void OpenInstance()
		{
			if (!IsValid)
			{
				Agent.ToggleInstance();
			}
		}

		public static new void Close()
		{
			if (IsOpen)
			{
				Agent.ToggleInstance();
			}
		}
	}
}