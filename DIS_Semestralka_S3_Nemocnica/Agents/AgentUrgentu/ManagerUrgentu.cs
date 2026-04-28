using OSPABA;
using Simulation;
using Agents.AgentUrgentu.InstantAssistants;
using AgentZdrojovType = Agents.AgentZdrojov.AgentZdrojov;

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
				((ZaradenieDoRaduVstupneVysetrenie)MyAgent.FindAssistant(SimId.ZaradenieDoRaduVstupneVysetrenie)).Execute(message);
				SkusSpustitVstupneVysetrenie();
			}
		}

		//meta! sender="AgentZdrojov", id="28", type="Response"
		public void ProcessPridelenieZdrojovVstupneVysetrenie(MessageForm message)
		{
			Sim.AktualizujStavPacienta(((MyMessage)message).PacientId, "Presun sestry");
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

			// Uvoľni VV zdroje priamo a skús spustiť čakajúcich
			var z = Z;
			z.VolneSestry++;
			z.VolneMiestnostiB++;

			// Zaraď do radu ošetrenia
			msg.JePresunNaOsetrenie = true;
			((ZaradenieDoRaduOsetrenie)MyAgent.FindAssistant(SimId.ZaradenieDoRaduOsetrenie)).Execute(message);

			SkusSpustitVstupneVysetrenie();
			SkusSpustitOsetrenie();
		}

		//meta! sender="AgentZdrojov", id="27", type="Response"
		public void ProcessPridelenieZdrojovOsetrenie(MessageForm message)
		{
			var msg2 = (MyMessage)message;
			Sim.AktualizujStavPacienta(msg2.PacientId, "Presun personálu");
			Sim.AktualizujMiestnostPacienta(msg2.PacientId, msg2.PouzilaMiestnostA);
			message.Code = Mc.PresunPersonalu;
			message.Addressee = MySim.FindAgent(SimId.AgentPresunov);
			Request(message);
		}

		//meta! sender="AgentOsetrenia", id="24", type="Response"
		public void ProcessVykonanieOsetrenia(MessageForm message)
		{
			var msg = (MyMessage)message;
			Sim.AktualizujStavPacienta(msg.PacientId, "Odchod");

			// Uvoľni ošetrovacie zdroje priamo a skús spustiť čakajúcich
			var z = Z;
			z.VolneLekari++;
			z.VolneSestry++;
			if (msg.PouzilaMiestnostA) z.VolneMiestnostiA++;
			else                       z.VolneMiestnostiB++;

			SkusSpustitVstupneVysetrenie();
			SkusSpustitOsetrenie();

			msg.JeOdchod = true;
			message.Code = Mc.PresunPacienta;
			message.Addressee = MySim.FindAgent(SimId.AgentPresunov);
			Request(message);
		}

		// ── Dispatch helpers ─────────────────────────────────────────────

		private AgentZdrojovType Z => (AgentZdrojovType)MySim.FindAgent(SimId.AgentZdrojov);

		private void SkusSpustitVstupneVysetrenie()
		{
			var z = Z;
			if (MyAgent.RadVV.Count == 0 || z.VolneSestry == 0 || z.VolneMiestnostiB == 0) return;

			var msg = MyAgent.RadVV.Dequeue();
			MyAgent.RadVVIds.Remove(msg.PacientId);
			Sim.LocDobaVV.AddValue(MySim.CurrentTime - msg.CasVstupuDoRadu);

			msg.Code = Mc.PridelenieZdrojovVstupneVysetrenie;
			msg.Addressee = MySim.FindAgent(SimId.AgentZdrojov);
			Request(msg);  // AgentZdrojov alokuje zdroje cez PriradenieZdrojovPreVstupneVysetrenie
		}

		private void SkusSpustitOsetrenie()
		{
			var z = Z;
			if (MyAgent.RadOsetrenie.Count == 0 || z.VolneLekari == 0 || z.VolneSestry == 0) return;

			MyAgent.RadOsetrenie.TryPeek(out var pacient, out _);
			bool pouzijA;
			if (pacient!.Priorita <= 2)
			{
				if (z.VolneMiestnostiA == 0) return;
				pouzijA = true;
			}
			else if (pacient.Priorita <= 4)
			{
				if      (z.VolneMiestnostiB > 0) pouzijA = false;
				else if (z.VolneMiestnostiA > 0) pouzijA = true;
				else return;
			}
			else
			{
				if (z.VolneMiestnostiB == 0) return;
				pouzijA = false;
			}

			MyAgent.RadOsetrenie.Dequeue();
			MyAgent.RadOsetreniaItems.RemoveAll(x => x.Id == pacient.PacientId);
			Sim.LocDobaOsetrenie.AddValue(MySim.CurrentTime - pacient.CasVstupuDoRadu);
			pacient.PouzilaMiestnostA = pouzijA;

			pacient.Code = Mc.PridelenieZdrojovOsetrenie;
			pacient.Addressee = MySim.FindAgent(SimId.AgentZdrojov);
			Request(pacient);  // AgentZdrojov alokuje zdroje cez PriradenieZdrojovPreOsetrenie
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
