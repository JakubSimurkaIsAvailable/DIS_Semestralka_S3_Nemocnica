using OSPABA;
using Simulation;

namespace Agents.AgentUrgentu
{
	//meta! id="3"
	public class ManagerUrgentu : OSPABA.Manager
	{
		public ManagerUrgentu(int id, OSPABA.Simulation mySim, Agent myAgent) :
			base(id, mySim, myAgent)
		{
			Init();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();

			if (PetriNet != null)
			{
				PetriNet.Clear();
			}
		}

		//meta! sender="AgentModelu", id="8", type="Request"
		public void ProcessVysetreniePacienta(MessageForm message)
		{
			message.Code = Mc.PrichodPacientaNaUrgent;
			message.Addressee = MySim.FindAgent(SimId.AgentPresunov);
			Request(message);
		}

		//meta! sender="AgentVstupnehoVysetrenia", id="22", type="Response"
		public void ProcessVykonanieVstupnehoVysetrenia(MessageForm message)
		{
			// uvolni zdroje vstupneho vysetrenia
			var release = new MyMessage(MySim)
			{
				Code = Mc.UvolnenieZdrojovVstupneVysetrenie,
				Addressee = MySim.FindAgent(SimId.AgentZdrojov)
			};
			Notice(release);

			// ziadaj zdroje pre lekarske osetrenie
			message.Code = Mc.PridelenieZdrojovOsetrenie;
			message.Addressee = MySim.FindAgent(SimId.AgentZdrojov);
			Request(message);
		}

		//meta! sender="AgentZdrojov", id="28", type="Response"
		public void ProcessPridelenieZdrojovVstupneVysetrenie(MessageForm message)
		{
			message.Code = Mc.VykonanieVstupnehoVysetrenia;
			message.Addressee = MySim.FindAgent(SimId.AgentVstupnehoVysetrenia);
			Request(message);
		}

		//meta! sender="AgentZdrojov", id="27", type="Response"
		public void ProcessPridelenieZdrojovOsetrenie(MessageForm message)
		{
			message.Code = Mc.VykonanieOsetrenia;
			message.Addressee = MySim.FindAgent(SimId.AgentOsetrenia);
			Request(message);
		}

		//meta! sender="AgentPresunov", id="77", type="Response"
		public void ProcessPresunPacienta(MessageForm message)
		{
		}

		//meta! sender="AgentOsetrenia", id="24", type="Response"
		public void ProcessVykonanieOsetrenia(MessageForm message)
		{
			// uvolni zdroje osetrenia
			var release = new MyMessage(MySim)
			{
				PouzilaMiestnostA = ((MyMessage)message).PouzilaMiestnostA,
				Code = Mc.UvolnenieZdrojovOsetrenie,
				Addressee = MySim.FindAgent(SimId.AgentZdrojov)
			};
			Notice(release);
			Response(message);
		}

		//meta! sender="AgentPresunov", id="78", type="Response"
		public void ProcessPresunPersonalu(MessageForm message)
		{
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
			case Mc.PrichodPacientaNaUrgent:
				message.Code = Mc.PridelenieZdrojovVstupneVysetrenie;
				message.Addressee = MySim.FindAgent(SimId.AgentZdrojov);
				Request(message);
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
			case Mc.PridelenieZdrojovVstupneVysetrenie:
				ProcessPridelenieZdrojovVstupneVysetrenie(message);
			break;

			case Mc.PresunPersonalu:
				ProcessPresunPersonalu(message);
			break;

			case Mc.VysetreniePacienta:
				ProcessVysetreniePacienta(message);
			break;

			case Mc.VykonanieVstupnehoVysetrenia:
				ProcessVykonanieVstupnehoVysetrenia(message);
			break;

			case Mc.PridelenieZdrojovOsetrenie:
				ProcessPridelenieZdrojovOsetrenie(message);
			break;

			case Mc.VykonanieOsetrenia:
				ProcessVykonanieOsetrenia(message);
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
		public new AgentUrgentu MyAgent
		{
			get
			{
				return (AgentUrgentu)base.MyAgent;
			}
		}
	}
}
