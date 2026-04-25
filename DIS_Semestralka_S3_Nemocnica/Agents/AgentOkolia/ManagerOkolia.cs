using OSPABA;
using Simulation;

namespace Agents.AgentOkolia
{
	//meta! id="2"
	public class ManagerOkolia : OSPABA.Manager
	{
		public ManagerOkolia(int id, OSPABA.Simulation mySim, Agent myAgent) :
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

		//meta! sender="PrichodPacientaSanitka", id="57", type="Finish"
		public void ProcessFinishPrichodPacientaSanitka(MessageForm message)
		{
			message.Code = Mc.PrichodPacienta;
			message.Addressee = MySim.FindAgent(SimId.AgentModelu);
			Notice(message);
        }

		//meta! sender="PrichodPacienta", id="55", type="Finish"
		public void ProcessFinishPrichodPacienta(MessageForm message)
		{
			message.Code = Mc.PrichodPacienta;
			message.Addressee = MySim.FindAgent(SimId.AgentModelu);
            Notice(message);
		}

		//meta! sender="AgentModelu", id="5", type="Notice"
		public void ProcessOdchodPacienta(MessageForm message)
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
			case Mc.OdchodPacienta:
				ProcessOdchodPacienta(message);
			break;

			case Mc.Finish:
				switch (message.Sender.Id)
				{
				case SimId.PrichodPacientaSanitka:
					ProcessFinishPrichodPacientaSanitka(message);
				break;

				case SimId.PrichodPacienta:
					ProcessFinishPrichodPacienta(message);
				break;
				}
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new AgentOkolia MyAgent
		{
			get
			{
				return (AgentOkolia)base.MyAgent;
			}
		}
	}
}
