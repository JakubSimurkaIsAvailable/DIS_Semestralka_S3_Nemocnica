using OSPABA;
using Simulation;
using Simulation.Resources;
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
			ZaznamVytazenosti();
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
            SkusSpustitOsetrenie();
            SkusSpustitVV();
        }

		private void SkusSpustitVV()
		{
			if (MyAgent.RadVV.Count == 0 || MyAgent.SestryVolne.Count == 0 || MyAgent.MiestnostiBVolne.Count == 0) return;

			var msg = MyAgent.RadVV.Dequeue();
			MyAgent.RadVVIds.Remove(msg.PacientId);

			msg.PridelenaMiestnost = MyAgent.MiestnostiBVolne.Dequeue();
			((PriradenieZdrojovPreVstupneVysetrenie)MyAgent.FindAssistant(SimId.PriradenieZdrojovPreVstupneVysetrenie)).Execute(msg);
			ZaznamVytazenosti();

			msg.Code = Mc.ZdrojePrideleneVV;
			msg.Addressee = MySim.FindAgent(SimId.AgentUrgentu);
			Notice(msg);
		}

		private void SkusSpustitOsetrenie()
		{
			if (MyAgent.LekariVolne.Count == 0 || MyAgent.SestryVolne.Count == 0) return;

			// RadA (priorita 1-2): len miestnosť A
			if (MyAgent.RadA.Count > 0 && MyAgent.MiestnostiAVolne.Count > 0)
			{
				ServeOsetrenie(MyAgent.RadA, MyAgent.RadAItems, useA: true, bucket: 0);
				return;
			}

			// RadAB (priorita 3-4): preferovane B, fallback A ak RadA prázdne
			if (MyAgent.RadAB.Count > 0)
			{
				bool useA;
				if      (MyAgent.MiestnostiBVolne.Count > 0)                                  useA = false;
				else if (MyAgent.MiestnostiAVolne.Count > 0 && MyAgent.RadA.Count == 0) useA = true;
				else    return;

				ServeOsetrenie(MyAgent.RadAB, MyAgent.RadABItems, useA, bucket: 1);
				return;
			}

			// RadB (priorita 5): len miestnosť B
			if (MyAgent.RadB.Count > 0 && MyAgent.MiestnostiBVolne.Count > 0)
			{
				ServeOsetrenie(MyAgent.RadB, MyAgent.RadBItems, useA: false, bucket: 2);
			}
		}

		private void ServeOsetrenie(
			PriorityQueue<MyMessage, (int, int)> rad,
			List<(int Id, int Priorita)> items,
			bool useA,
			int bucket)
		{
			var msg = rad.Dequeue();
			items.RemoveAll(x => x.Id == msg.PacientId);
			msg.OsetrenieBucket = bucket;
			msg.PridelenaMiestnost = useA
				? (Miestnost)MyAgent.MiestnostiAVolne.Dequeue()
				: MyAgent.MiestnostiBVolne.Dequeue();
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
			a.LocVytazenostLekari.AddWeightedValue(
				a.TotalLekari > 0 ? (double)(a.TotalLekari - a.VolneLekari) / a.TotalLekari : 0, t);
			a.LocVytazenostSestry.AddWeightedValue(
				a.TotalSestry > 0 ? (double)(a.TotalSestry - a.VolneSestry) / a.TotalSestry : 0, t);
			a.LocVytazenostMiestnostiA.AddWeightedValue(
				a.TotalMiestnostiA > 0 ? (double)(a.TotalMiestnostiA - a.VolneMiestnostiA) / a.TotalMiestnostiA : 0, t);
			a.LocVytazenostMiestnostiB.AddWeightedValue(
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
