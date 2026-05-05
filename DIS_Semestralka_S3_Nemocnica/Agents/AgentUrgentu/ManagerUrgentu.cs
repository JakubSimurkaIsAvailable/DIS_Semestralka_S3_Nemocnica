using OSPABA;
using Simulation;

namespace Agents.AgentUrgentu
{
	//meta! id="3"
	public class ManagerUrgentu : OSPABA.Manager
	{
		private MySimulation Sim => (MySimulation)MySim;

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
			Sim.AktualizujStavPacienta(((MyMessage)message).PacientId, "Presun na VV");
			message.Code = Mc.PresunPacienta;
			message.Addressee = MySim.FindAgent(SimId.AgentPresunov);
			Request(message);
		}

		//meta! sender="AgentPresunov", id="77", type="Response"
		public void ProcessPresunPacienta(MessageForm message)
		{
			var msg = (MyMessage)message;
			if (msg.JeOdchod)
			{
				message.Code = Mc.VysetreniePacienta;
				Response(message);
			}
			else
			{
				Sim.AktualizujStavPacienta(msg.PacientId, "Čaká na VV");
				Sim.AnimPacientDoVVRadu(msg.PacientId);
				message.Code = Mc.ZaradenieDoRaduVV;
				message.Addressee = MySim.FindAgent(SimId.AgentZdrojov);
				Notice(message);
			}
		}

		//meta! sender="AgentZdrojov", id="129", type="Notice"
		public void ProcessZdrojePrideleneVV(MessageForm message)
		{
			var msg = (MyMessage)message;
			Sim.AktualizujStavPacienta(msg.PacientId, "Presun sestry");
			Sim.AnimAllocVVRoom(msg.PacientId, msg.PriradenaSestrа!, (Simulation.Resources.MiestnostB)msg.PridelenaMiestnost!);
			message.Code = Mc.PresunPersonalu;
			message.Addressee = MySim.FindAgent(SimId.AgentPresunov);
			Request(message);
		}

		//meta! sender="AgentPresunov", id="78", type="Response"
		public void ProcessPresunPersonalu(MessageForm message)
		{
			var msg = (MyMessage)message;
			if (msg.JePresunNaOsetrenie)
			{
				Sim.AktualizujStavPacienta(msg.PacientId, "Ošetrenie prebieha");
				message.Code = Mc.VykonanieOsetrenia;
				message.Addressee = MySim.FindAgent(SimId.AgentOsetrenia);
			}
			else
			{
				Sim.AktualizujStavPacienta(msg.PacientId, "VV prebieha");
				message.Code = Mc.VykonanieVstupnehoVysetrenia;
				message.Addressee = MySim.FindAgent(SimId.AgentVstupnehoVysetrenia);
			}
			Request(message);
		}

		//meta! sender="AgentVstupnehoVysetrenia", id="22", type="Response"
		public void ProcessVykonanieVstupnehoVysetrenia(MessageForm message)
		{
			var msg = (MyMessage)message;
			Sim.AktualizujPriorituPacienta(msg.PacientId, msg.Priorita);
			Sim.AktualizujStavPacienta(msg.PacientId, "Čaká na ošetrenie");
			Sim.AnimUvolniVV(msg.PacientId);

			var uvolni = new MyMessage(MySim);
			uvolni.PriradenaSestrа    = msg.PriradenaSestrа;
			uvolni.PridelenaMiestnost = msg.PridelenaMiestnost;
			uvolni.JePresunNaOsetrenie = false;
			uvolni.Code = Mc.UvolnenieAmbulancie;
			uvolni.Addressee = MySim.FindAgent(SimId.AgentZdrojov);
			Notice(uvolni);

			msg.JePresunNaOsetrenie = true;
			message.Code = Mc.ZaradenieDoRaduOsetrenie;
			message.Addressee = MySim.FindAgent(SimId.AgentZdrojov);
			Notice(message);
		}

		//meta! sender="AgentZdrojov", id="130", type="Notice"
		public void ProcessZdrojePrideleneOsetrenie(MessageForm message)
		{
			var msg = (MyMessage)message;
			Sim.AktualizujStavPacienta(msg.PacientId, "Presun personálu");
			Sim.AktualizujMiestnostPacienta(msg.PacientId, msg.PouzilaMiestnostA);
			Sim.AnimAllocOsetrenieRoom(msg.PacientId, msg.PridelenaMiestnost!, msg.PriradenaSestrа!, msg.PriradenyLekar!);
			message.Code = Mc.PresunPersonalu;
			message.Addressee = MySim.FindAgent(SimId.AgentPresunov);
			Request(message);
		}

		//meta! sender="AgentOsetrenia", id="24", type="Response"
		public void ProcessVykonanieOsetrenia(MessageForm message)
		{
			var msg = (MyMessage)message;
			Sim.AktualizujStavPacienta(msg.PacientId, "Odchod");
			Sim.AnimUvolniOsetrenie(msg.PacientId);

			var uvolni = new MyMessage(MySim);
			uvolni.PriradenaSestrа    = msg.PriradenaSestrа;
			uvolni.PriradenyLekar     = msg.PriradenyLekar;
			uvolni.PridelenaMiestnost = msg.PridelenaMiestnost;
			uvolni.JePresunNaOsetrenie = true;
			uvolni.Code = Mc.UvolnenieAmbulancie;
			uvolni.Addressee = MySim.FindAgent(SimId.AgentZdrojov);
			Notice(uvolni);

			msg.JeOdchod = true;
			message.Code = Mc.PresunPacienta;
			message.Addressee = MySim.FindAgent(SimId.AgentPresunov);
			Request(message);
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
			}
		}

		private string Cas() => TimeSpan.FromSeconds(MySim.CurrentTime).ToString(@"hh\:mm\:ss");

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

			case Mc.PresunPacienta:
				ProcessPresunPacienta(message);
			break;

			case Mc.ZdrojePrideleneVV:
				ProcessZdrojePrideleneVV(message);
			break;

			case Mc.PresunPersonalu:
				ProcessPresunPersonalu(message);
			break;

			case Mc.VykonanieVstupnehoVysetrenia:
				ProcessVykonanieVstupnehoVysetrenia(message);
			break;

			case Mc.ZdrojePrideleneOsetrenie:
				ProcessZdrojePrideleneOsetrenie(message);
			break;

			case Mc.VykonanieOsetrenia:
				ProcessVykonanieOsetrenia(message);
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
