using OSPABA;
using Simulation;

namespace Agents.AgentOsetrenia
{
	//meta! id="11"
	public class ManagerOsetrenia : OSPABA.Manager
	{
		public ManagerOsetrenia(int id, OSPABA.Simulation mySim, Agent myAgent) :
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

		//meta! sender="AgentUrgentu", id="26", type="Notice"
		public void ProcessUvolnenieAmbulancie(MessageForm message)
		{
		}

		//meta! sender="AgentUrgentu", id="24", type="Request"
		public void ProcessVykonanieOsetrenia(MessageForm message)
		{
		}

		//meta! sender="ProcessOsetrenie", id="41", type="Finish"
		public void ProcessFinish(MessageForm message)
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

			case Mc.VykonanieOsetrenia:
				ProcessVykonanieOsetrenia(message);
			break;

			case Mc.Finish:
				ProcessFinish(message);
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new AgentOsetrenia MyAgent
		{
			get
			{
				return (AgentOsetrenia)base.MyAgent;
			}
		}
	}
}
