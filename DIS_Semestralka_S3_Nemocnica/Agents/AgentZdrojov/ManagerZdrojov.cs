using OSPABA;
using Simulation;
using Agents.AgentZdrojov.InstantAssistants;

namespace Agents.AgentZdrojov
{
	//meta! id="12"
	public class ManagerZdrojov : OSPABA.Manager
	{
		private MySimulation Sim => (MySimulation)MySim;

		public ManagerZdrojov(int id, OSPABA.Simulation mySim, Agent myAgent) :
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

		//meta! sender="AgentUrgentu", id="127", type="Notice"
		public void ProcessZaradenieDoRaduVV(MessageForm message)
		{
			((ZaradenieDoRaduVstupneVysetrenie)MyAgent.FindAssistant(SimId.ZaradenieDoRaduVstupneVysetrenie)).Execute(message);
			SkusSpustitVV();
		}

		//meta! sender="AgentUrgentu", id="128", type="Notice"
		public void ProcessZaradenieDoRaduOsetrenie(MessageForm message)
		{
			((ZaradenieDoRaduOsetrenie)MyAgent.FindAssistant(SimId.ZaradenieDoRaduOsetrenie)).Execute(message);
			SkusSpustitOsetrenie();
		}

		//meta! sender="AgentUrgentu", type="Notice"
		public void ProcessUvolnenieZdrojov(MessageForm message)
		{
			((UvolnenieZdrojov)MyAgent.FindAssistant(SimId.UvolnenieZdrojov)).Execute(message);
			SkusSpustitVV();
			SkusSpustitOsetrenie();
		}

		//meta! sender="AgentUrgentu", id="98", type="Notice"
		public void ProcessUvolnenieAmbulancie(MessageForm message)
		{
		}

		private void SkusSpustitVV()
		{
			if (MyAgent.RadVV.Count == 0 || MyAgent.VolneSestry == 0 || MyAgent.VolneMiestnostiB == 0) return;

			var msg = MyAgent.RadVV.Dequeue();
			MyAgent.RadVVIds.Remove(msg.PacientId);
			Sim.LocDobaVV.AddValue(MySim.CurrentTime - msg.CasVstupuDoRadu);

			((PriradenieZdrojovPreVstupneVysetrenie)MyAgent.FindAssistant(SimId.PriradenieZdrojovPreVstupneVysetrenie)).Execute(msg);

			msg.Code = Mc.ZdrojePrideleneVV;
			msg.Addressee = MySim.FindAgent(SimId.AgentUrgentu);
			Notice(msg);
		}

		private void SkusSpustitOsetrenie()
		{
			if (MyAgent.RadOsetrenie.Count == 0 || MyAgent.VolneLekari == 0 || MyAgent.VolneSestry == 0) return;

			MyAgent.RadOsetrenie.TryPeek(out var pacient, out _);
			bool pouzijA;
			if (pacient!.Priorita <= 2)
			{
				if (MyAgent.VolneMiestnostiA == 0) return;
				pouzijA = true;
			}
			else if (pacient.Priorita <= 4)
			{
				if      (MyAgent.VolneMiestnostiB > 0) pouzijA = false;
				else if (MyAgent.VolneMiestnostiA > 0) pouzijA = true;
				else return;
			}
			else
			{
				if (MyAgent.VolneMiestnostiB == 0) return;
				pouzijA = false;
			}

			MyAgent.RadOsetrenie.Dequeue();
			MyAgent.RadOsetreniaItems.RemoveAll(x => x.Id == pacient.PacientId);
			Sim.LocDobaOsetrenie.AddValue(MySim.CurrentTime - pacient.CasVstupuDoRadu);
			pacient.PouzilaMiestnostA = pouzijA;

			((PriradenieZdrojovPreOsetrenie)MyAgent.FindAssistant(SimId.PriradenieZdrojovPreOsetrenie)).Execute(pacient);

			pacient.Code = Mc.ZdrojePrideleneOsetrenie;
			pacient.Addressee = MySim.FindAgent(SimId.AgentUrgentu);
			Notice(pacient);
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
			case Mc.ZaradenieDoRaduVV:
				ProcessZaradenieDoRaduVV(message);
			break;

			case Mc.ZaradenieDoRaduOsetrenie:
				ProcessZaradenieDoRaduOsetrenie(message);
			break;

			case Mc.UvolnenieZdrojovVstupneVysetrenie:
			case Mc.UvolnenieZdrojovOsetrenie:
				ProcessUvolnenieZdrojov(message);
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
		public new AgentZdrojov MyAgent
		{
			get
			{
				return (AgentZdrojov)base.MyAgent;
			}
		}
	}
}
