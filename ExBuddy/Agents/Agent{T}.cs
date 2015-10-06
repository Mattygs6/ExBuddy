namespace ExBuddy.Agents
{
	using System;

	using ff14bot.Managers;

	public abstract class Agent<T>
		where T : Agent<T>, new()
	{
		private readonly AgentInterface agentInterface;

		protected Agent(int id)
		{
			Id = id;
			this.agentInterface = AgentModule.GetAgentInterfaceById(id);
		}

		public AgentInterface AgentInterface
		{
			get
			{
				return this.agentInterface;
			}
		}

		public int Id { get; private set; }

		public static T Instance
		{
			get
			{
				return new T();
			}
		}

		public IntPtr Pointer
		{
			get
			{
				return this.agentInterface.Pointer;
			}
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