using OSPABA;
using Simulation;
using Agents.AgentZdrojov.InstantAssistants;
using DIS_Semestralka_S3_Nemocnica.Collectors;

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
			ZaznamVytazenosti(); // initialize weighted utilization at t=0
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

		//meta! sender="AgentUrgentu", id="98", type="Notice"
		public void ProcessUvolnenieAmbulancie(MessageForm message)
		{
            ((UvolnenieZdrojov)MyAgent.FindAssistant(SimId.UvolnenieZdrojov)).Execute(message);
            ZaznamVytazenosti();
            SkusSpustitVV();
            SkusSpustitOsetrenie();
        }

		private void SkusSpustitVV()
		{
			if (MyAgent.RadVV.Count == 0 || MyAgent.VolneSestry == 0 || MyAgent.VolneMiestnostiB == 0) return;

			var msg = MyAgent.RadVV.Dequeue();
			MyAgent.RadVVIds.Remove(msg.PacientId);
			double wait = MySim.CurrentTime - msg.CasVstupuDoRadu;
			Sim.LocDobaVV.AddValue(wait);
			if (msg.PrisielSanitkou)
				Sim.LocDobaVVSanitka.AddValue(wait);
			else
				Sim.LocDobaVVPeso.AddValue(wait);

			((PriradenieZdrojovPreVstupneVysetrenie)MyAgent.FindAssistant(SimId.PriradenieZdrojovPreVstupneVysetrenie)).Execute(msg);
			ZaznamVytazenosti();

			msg.Code = Mc.ZdrojePrideleneVV;
			msg.Addressee = MySim.FindAgent(SimId.AgentUrgentu);
			Notice(msg);
		}

		private void SkusSpustitOsetrenie()
		{
			if (MyAgent.VolneLekari == 0 || MyAgent.VolneSestry == 0) return;

			// RadA (priorita 1-2): len miestnosť A
			if (MyAgent.RadA.Count > 0 && MyAgent.VolneMiestnostiA > 0)
			{
				ServeOsetrenie(MyAgent.RadA, MyAgent.RadAItems, true, Sim.LocDobaOsetrenieA);
				return;
			}

			// RadAB (priorita 3-4): preferovane B, fallback A ak RadA prázdne
			if (MyAgent.RadAB.Count > 0)
			{
				bool useA;
				if      (MyAgent.VolneMiestnostiB > 0)                             useA = false;
				else if (MyAgent.VolneMiestnostiA > 0 && MyAgent.RadA.Count == 0) useA = true;
				else    return;

				ServeOsetrenie(MyAgent.RadAB, MyAgent.RadABItems, useA, Sim.LocDobaOsetrenieAB);
				return;
			}

			// RadB (priorita 5): len miestnosť B
			if (MyAgent.RadB.Count > 0 && MyAgent.VolneMiestnostiB > 0)
			{
				ServeOsetrenie(MyAgent.RadB, MyAgent.RadBItems, false, Sim.LocDobaOsetrenieB);
			}
		}

		private void ServeOsetrenie(
			PriorityQueue<MyMessage, (int, int)> rad,
			List<(int Id, int Priorita)> items,
			bool pouzijA,
			StatisticsCollector specificStat)
		{
			var msg = rad.Dequeue();
			items.RemoveAll(x => x.Id == msg.PacientId);
			double wait = MySim.CurrentTime - msg.CasVstupuDoRadu;
			Sim.LocDobaOsetrenie.AddValue(wait);
			specificStat.AddValue(wait);
			if (Sim.Pacienti.TryGetValue(msg.PacientId, out var pacInfo))
			{
				double dobaPDO = MySim.CurrentTime - pacInfo.CasPrichodu;
				Sim.LocDobaPrichodDoOsetrenia.AddValue(dobaPDO);
				if (pacInfo.PrisielSanitkou)
					Sim.LocDobaPrichodDoOsetreniaSanitka.AddValue(dobaPDO);
				else
					Sim.LocDobaPrichodDoOsetreniaPeso.AddValue(dobaPDO);
			}
			msg.PouzilaMiestnostA = pouzijA;
			((PriradenieZdrojovPreOsetrenie)MyAgent.FindAssistant(SimId.PriradenieZdrojovPreOsetrenie)).Execute(msg);
			ZaznamVytazenosti();
			msg.Code = Mc.ZdrojePrideleneOsetrenie;
			msg.Addressee = MySim.FindAgent(SimId.AgentUrgentu);
			Notice(msg);
		}

		private void ZaznamVytazenosti()
		{
			var a = MyAgent;
			double t = MySim.CurrentTime;
			Sim.LocVytazenostLekari.AddWeightedValue(
				a.TotalLekari > 0 ? (double)(a.TotalLekari - a.VolneLekari) / a.TotalLekari : 0, t);
			Sim.LocVytazenostSestry.AddWeightedValue(
				a.TotalSestry > 0 ? (double)(a.TotalSestry - a.VolneSestry) / a.TotalSestry : 0, t);
			Sim.LocVytazenostMiestnostiA.AddWeightedValue(
				a.TotalMiestnostiA > 0 ? (double)(a.TotalMiestnostiA - a.VolneMiestnostiA) / a.TotalMiestnostiA : 0, t);
			Sim.LocVytazenostMiestnostiB.AddWeightedValue(
				a.TotalMiestnostiB > 0 ? (double)(a.TotalMiestnostiB - a.VolneMiestnostiB) / a.TotalMiestnostiB : 0, t);
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
