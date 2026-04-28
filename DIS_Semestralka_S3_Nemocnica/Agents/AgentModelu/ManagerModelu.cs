using OSPABA;
using Simulation;

namespace Agents.AgentModelu
{
	//meta! id="1"
	public class ManagerModelu : OSPABA.Manager
	{
		public ManagerModelu(int id, OSPABA.Simulation mySim, Agent myAgent) :
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

		private MySimulation Sim => (MySimulation)MySim;

		//meta! sender="AgentUrgentu", id="8", type="Response"
		public void ProcessVysetreniePacienta(MessageForm message)
		{
			var msg = (MyMessage)message;
			Console.WriteLine($"[{Cas()}] Pacient #{msg.PacientId} opustil urgent");
			message.Code = Mc.OdchodPacienta;
			message.Addressee = MySim.FindAgent(SimId.AgentOkolia);
			Notice(message);
		}

		private string Cas() =>
			TimeSpan.FromSeconds(MySim.CurrentTime).ToString(@"hh\:mm\:ss");

		//meta! sender="AgentOkolia", id="6", type="Notice"
		public void ProcessPrichodPacienta(MessageForm message)
		{
			message.Code = Mc.VysetreniePacienta;
			message.Addressee = MySim.FindAgent(SimId.AgentUrgentu);
			Request(message);
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
			case Mc.VysetreniePacienta:
				ProcessVysetreniePacienta(message);
			break;

			case Mc.PrichodPacienta:
				ProcessPrichodPacienta(message);
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new AgentModelu MyAgent
		{
			get
			{
				return (AgentModelu)base.MyAgent;
			}
		}
	}
}