using OSPABA;
using Simulation;

namespace Agents.AgentPresunov
{
	//meta! id="68"
	public class ManagerPresunov : OSPABA.Manager
	{
		public ManagerPresunov(int id, OSPABA.Simulation mySim, Agent myAgent) :
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

		//meta! sender="AgentUrgentu", id="77", type="Request"
		public void ProcessPresunPacienta(MessageForm message)
		{
		}

		//meta! sender="AgentUrgentu", id="78", type="Request"
		public void ProcessPresunPersonalu(MessageForm message)
		{
		}

		//meta! sender="ProcessPresunitiaPersonalu", id="76", type="Finish"
		public void ProcessFinishProcessPresunitiaPersonalu(MessageForm message)
		{
		}

		//meta! sender="ProcessPresunutiaPacienta", id="74", type="Finish"
		public void ProcessFinishProcessPresunutiaPacienta(MessageForm message)
		{
			Response(message);
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
			case Mc.PrichodPacientaNaUrgent:
				message.Addressee = MyAgent.FindAssistant(SimId.ProcessPresunutiaPacienta);
				StartContinualAssistant(message);
				break;
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
				switch (message.Sender.Id)
				{
				case SimId.ProcessPresunitiaPersonalu:
					ProcessFinishProcessPresunitiaPersonalu(message);
				break;

				case SimId.ProcessPresunutiaPacienta:
					ProcessFinishProcessPresunutiaPacienta(message);
				break;
				}
			break;

			case Mc.PresunPersonalu:
				ProcessPresunPersonalu(message);
			break;

			case Mc.PresunPacienta:
				ProcessPresunPacienta(message);
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new AgentPresunov MyAgent
		{
			get
			{
				return (AgentPresunov)base.MyAgent;
			}
		}
	}
}
