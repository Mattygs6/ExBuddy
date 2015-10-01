namespace ExBuddy.Agents
{
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

		public static void Toggle()
		{
			new T().ToggleInstance();
		}

		public void ToggleInstance()
		{
			this.agentInterface.Toggle();
		}
	}
}