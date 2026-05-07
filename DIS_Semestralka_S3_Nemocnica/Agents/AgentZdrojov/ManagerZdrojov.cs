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

			// Meriame čakanie v rade tu – nie po príchode sestry (správny bod)
			double wait = MySim.CurrentTime - msg.CasVstupuDoRadu;
			MyAgent.LocDobaVV.AddValue(wait);
			if (msg.PrisielSanitkou)
				MyAgent.LocDobaVVSanitka.AddValue(wait);
			else
				MyAgent.LocDobaVVPeso.AddValue(wait);

			msg.PridelenaMiestnost = MyAgent.MiestnostiBVolne.Dequeue();
			((PriradenieZdrojovPreVstupneVysetrenie)MyAgent.FindAssistant(SimId.PriradenieZdrojovPreVstupneVysetrenie)).Execute(msg);
			ZaznamVytazenosti();

			msg.Code = Mc.ZdrojePrideleneVV;
			msg.Addressee = MySim.FindAgent(SimId.AgentUrgentu);
			Notice(msg);
		}

		private void SkusSpustitOsetrenie()
		{
			// Serve one RadA patient (priority 1-2) from a type A room if possible
			if (MyAgent.RadA.Count > 0 && MyAgent.MiestnostiAVolne.Count > 0
				&& MyAgent.LekariVolne.Count > 0 && MyAgent.SestryVolne.Count > 0)
			{
				ServeOsetrenie(MyAgent.RadA, MyAgent.RadAItems, useA: true);
			}

			// Also independently serve one RadB patient (priority 3-5) if resources allow
			if (MyAgent.RadB.Count > 0 && MyAgent.LekariVolne.Count > 0 && MyAgent.SestryVolne.Count > 0)
			{
				bool bVolna = MyAgent.MiestnostiBVolne.Count > 0;
				bool aVolna = MyAgent.MiestnostiAVolne.Count > 0;
				if (bVolna) { ServeOsetrenie(MyAgent.RadB, MyAgent.RadBItems, useA: false); }
				else if (aVolna && MyAgent.RadB.Peek().Priorita < 5) { ServeOsetrenie(MyAgent.RadB, MyAgent.RadBItems, useA: true); }
			}
		}

		private void ServeOsetrenie(
			PriorityQueue<MyMessage, (int, int)> rad,
			List<(int Id, int Priorita)> items,
			bool useA)
		{
			var msg = rad.Dequeue();
			items.RemoveAll(x => x.Id == msg.PacientId);
			msg.OsetrenieBucket = msg.Priorita <= 2 ? 0 : (msg.Priorita <= 4 ? 1 : 2);

			// Meriame čakanie v rade tu – nie po presune personálu (správny bod)
			double wait = MySim.CurrentTime - msg.CasVstupuDoRadu;
			MyAgent.LocDobaOsetrenie.AddValue(wait);
			(msg.OsetrenieBucket switch
			{
				0 => MyAgent.LocDobaOsetrenieA,
				1 => MyAgent.LocDobaOsetrenieAB,
				_ => MyAgent.LocDobaOsetrenieB
			}).AddValue(wait);

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
