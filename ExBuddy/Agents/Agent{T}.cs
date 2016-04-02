namespace ExBuddy.Agents
{
	using System;
	using ff14bot.Managers;

	public abstract class Agent<T>
		where T : Agent<T>, new()
	{
		protected Agent(int id)
		{
			Id = id;
			AgentInterface = AgentModule.GetAgentInterfaceById(id);
		}

		public AgentInterface AgentInterface { get; set; }

		public int Id { get; set; }

		public static T Instance
		{
			get { return new T(); }
		}

		public IntPtr Pointer
		{
			get { return AgentInterface.Pointer; }
		}

		public static void Toggle()
		{
			new T().ToggleInstance();
		}

		public void ToggleInstance()
		{
			AgentModule.ToggleAgentInterfaceById(Id);
		}
	}
}