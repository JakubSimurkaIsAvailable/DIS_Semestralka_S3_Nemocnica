using OSPABA;
using Simulation;

namespace Agents.AgentZdrojov
{
	//meta! id="12"
	public class ManagerZdrojov : OSPABA.Manager
	{
		public ManagerZdrojov(int id, OSPABA.Simulation mySim, Agent myAgent) :
			base(id, mySim, myAgent)
		{
			Init();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication

			if (PetriNet != null)
			{
				PetriNet.Clear();
			}
		}

		//meta! sender="AgentUrgentu", id="98", type="Notice"
		public void ProcessUvolnenieAmbulancie(MessageForm message)
		{
		}

		//meta! sender="AgentUrgentu", id="128", type="Notice"
		public void ProcessZaradenieDoRaduOsetrenie(MessageForm message)
		{
		}

		//meta! sender="AgentUrgentu", id="127", type="Notice"
		public void ProcessZaradenieDoRaduVV(MessageForm message)
		{
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
			}
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		public void Init()
		{
		}

		override public void ProcessMessage(MessageForm message)
		{
			switch (message.Code)
			{
			case Mc.UvolnenieAmbulancie:
				ProcessUvolnenieAmbulancie(message);
			break;

			case Mc.ZaradenieDoRaduOsetrenie:
				ProcessZaradenieDoRaduOsetrenie(message);
			break;

			case Mc.ZaradenieDoRaduVV:
				ProcessZaradenieDoRaduVV(message);
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new AgentZdrojov MyAgent
		{
			get
			{
				return (AgentZdrojov)base.MyAgent;
			}
		}
	}
}
