using OSPABA;
using Simulation;
using Agents.AgentVstupnehoVysetrenia.InstantAssistants;

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
			message.Addressee = MyAgent.FindAssistant(SimId.ProcessVstupneVysetrenie);
			StartContinualAssistant(message);
		}

		//meta! sender="AgentUrgentu", id="23", type="Notice"
		public void ProcessUvolnenieAmbulancie(MessageForm message)
		{
		}

		//meta! sender="ProcessVstupneVysetrenie", id="39", type="Finish"
		public void ProcessFinish(MessageForm message)
		{
			var akcia = (PriradeniePriority)MyAgent.FindAssistant(SimId.PriradeniePriority);
			akcia.Execute(message);
			Response(message);
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
			case Mc.Finish:
				ProcessFinish(message);
			break;

			case Mc.VykonanieVstupnehoVysetrenia:
				ProcessVykonanieVstupnehoVysetrenia(message);
			break;

			case Mc.UvolnenieAmbulancie:
				ProcessUvolnenieAmbulancie(message);
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
