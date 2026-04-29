using OSPABA;
using Simulation;

namespace Agents.AgentVstupnehoVysetrenia
{
	//meta! id="10"
	public class ManagerVstupnehoVysetrenia : OSPABA.Manager
	{
		public ManagerVstupnehoVysetrenia(int id, OSPABA.Simulation mySim, Agent myAgent) :
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

		//meta! sender="AgentUrgentu", id="22", type="Request"
		public void ProcessVykonanieVstupnehoVysetrenia(MessageForm message)
		{
		}

		//meta! sender="ProcessVstupneVysetrenie", id="39", type="Finish"
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
			case Mc.VykonanieVstupnehoVysetrenia:
				ProcessVykonanieVstupnehoVysetrenia(message);
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
		public new AgentVstupnehoVysetrenia MyAgent
		{
			get
			{
				return (AgentVstupnehoVysetrenia)base.MyAgent;
			}
		}
	}
}
