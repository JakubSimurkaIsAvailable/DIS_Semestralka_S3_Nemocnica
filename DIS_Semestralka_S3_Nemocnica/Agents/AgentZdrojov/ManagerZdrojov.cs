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
			if (Sim.PreferVV)
			{
				SkusSpustitVV();
				SkusSpustitOsetrenie();
			}
			else
			{
				SkusSpustitOsetrenie();
				SkusSpustitVV();
			}
		}

		private void SkusSpustitVV()
		{
			if (MyAgent.RadVV.Count == 0 || MyAgent.SestryVolne.Count == 0 || MyAgent.MiestnostiBVolne.Count == 0) return;

			var msg = MyAgent.RadVV.Dequeue();
			MyAgent.RadVVIds.Remove(msg.PacientId);

			// Meriame čakanie v rade tu – nie po príchode sestry (správny bod)
			double wait = MySim.CurrentTime - msg.CasVstupuDoRadu;
			MyAgent.LocDobaVV.AddValue(wait);
			MyAgent.LocDlzkaRaduVV.AddWeightedValue(MyAgent.RadVV.Count, MySim.CurrentTime);
			if (msg.PrisielSanitkou)
				MyAgent.LocDobaVVSanitka.AddValue(wait);
			else
				MyAgent.LocDobaVVPeso.AddValue(wait);

			if (Sim.MinPohybPersonalu)
				SelectVVSmart(msg);
			else
			{
				msg.PridelenaMiestnost = MyAgent.MiestnostiBVolne[0];
				MyAgent.MiestnostiBVolne.RemoveAt(0);
			}

			((PriradenieZdrojovPreVstupneVysetrenie)MyAgent.FindAssistant(SimId.PriradenieZdrojovPreVstupneVysetrenie)).Execute(msg);
			ZaznamVytazenosti();

			msg.Code = Mc.ZdrojePrideleneVV;
			msg.Addressee = MySim.FindAgent(SimId.AgentUrgentu);
			Notice(msg);
		}

		private void SelectVVSmart(MyMessage msg)
		{
			// Hľadáme sestru, ktorá je už v niektorej voľnej MiestnostiB
			foreach (var s in MyAgent.SestryVolne)
			{
				if (!MyAgent.SestraPoloha.TryGetValue(s.Id, out var pol) || pol is not MiestnostB mb) continue;
				int idx = MyAgent.MiestnostiBVolne.IndexOf(mb);
				if (idx < 0) continue;
				msg.PridelenaMiestnost = mb;
				MyAgent.MiestnostiBVolne.RemoveAt(idx);
				msg.PriradenaSestrа = s;
				MyAgent.SestryVolne.Remove(s);
				return;
			}
			// Fallback: FIFO
			msg.PridelenaMiestnost = MyAgent.MiestnostiBVolne[0];
			MyAgent.MiestnostiBVolne.RemoveAt(0);
		}

		private void SkusSpustitOsetrenie()
		{
			int volniLekari = MyAgent.LekariVolne.Count;
			int volneSestry = MyAgent.SestryVolne.Count;
			// Pri rezervácii: ošetrenie potrebuje aspoň 2 voľné zdroje daného typu
			int minLekarPreB  = Sim.RezervaLekarPreA  ? 2 : 1;
			int minSestraOsetr = Sim.RezervaSestraPreVV ? 2 : 1;

			bool radBPreferovat = Sim.PrefRadBEnabled && MyAgent.RadB.Count > Sim.PrefRadBPrah;

			// Serve one RadB patient first if its queue exceeded the configured threshold
			if (radBPreferovat && MyAgent.RadB.Count > 0 && MyAgent.MiestnostiBVolne.Count > 0
				&& volniLekari >= minLekarPreB && volneSestry >= minSestraOsetr)
			{
				ServeOsetrenie(MyAgent.RadB, MyAgent.RadBItems, useA: false);
				volniLekari--;
				volneSestry--;
				return;
			}

			// Serve one RadA patient (priority 1-2) from a type A room if possible
			if (MyAgent.RadA.Count > 0 && MyAgent.MiestnostiAVolne.Count > 0
				&& volniLekari > 0 && volneSestry >= minSestraOsetr)
			{
				ServeOsetrenie(MyAgent.RadA, MyAgent.RadAItems, useA: true);
				volniLekari--;
				volneSestry--;
				return;
            }

			// Serve one RadAB patient (priority 3-4) — preference A or B is configurable
			if (MyAgent.RadAB.Count > 0)
			{
				bool mozeB = MyAgent.MiestnostiBVolne.Count > 0 && volniLekari >= minLekarPreB && volneSestry >= minSestraOsetr;
				bool mozeA = MyAgent.MiestnostiAVolne.Count > 0 && volniLekari > 0            && volneSestry >= minSestraOsetr;
				bool useA;
				if (Sim.RadABPreferA)
					useA = mozeA ? true  : mozeB ? false : false;
				else
					useA = mozeB ? false : mozeA ? true  : false;
				if (mozeA || mozeB)
				{
					ServeOsetrenie(MyAgent.RadAB, MyAgent.RadABItems, useA: useA);
					volniLekari--;
					volneSestry--;
				}
				return;
            }

			// Serve one RadB patient (priority 5) only from a type B room
			if (!radBPreferovat && MyAgent.RadB.Count > 0 && MyAgent.MiestnostiBVolne.Count > 0
				&& volniLekari >= minLekarPreB && volneSestry >= minSestraOsetr)
			{
				ServeOsetrenie(MyAgent.RadB, MyAgent.RadBItems, useA: false);
				volniLekari--;
				volneSestry--;
				return;
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
			(msg.OsetrenieBucket switch
			{
				0 => MyAgent.LocDlzkaRaduA,
				1 => MyAgent.LocDlzkaRaduAB,
				_ => MyAgent.LocDlzkaRaduB
			}).AddWeightedValue(msg.OsetrenieBucket switch
			{
				0 => MyAgent.RadA.Count,
				1 => MyAgent.RadAB.Count,
				_ => MyAgent.RadB.Count
			}, MySim.CurrentTime);

			if (Sim.MinPohybPersonalu)
				SelectOsetrenieSmart(msg, useA);
			else
			{
				msg.PridelenaMiestnost = useA
					? (Miestnost)MyAgent.MiestnostiAVolne[0]
					: MyAgent.MiestnostiBVolne[0];
				if (useA) MyAgent.MiestnostiAVolne.RemoveAt(0);
				else      MyAgent.MiestnostiBVolne.RemoveAt(0);
			}

			((PriradenieZdrojovPreOsetrenie)MyAgent.FindAssistant(SimId.PriradenieZdrojovPreOsetrenie)).Execute(msg);
			ZaznamVytazenosti();
			msg.Code = Mc.ZdrojePrideleneOsetrenie;
			msg.Addressee = MySim.FindAgent(SimId.AgentUrgentu);
			Notice(msg);
		}

		private void SelectOsetrenieSmart(MyMessage msg, bool useA)
		{
			// Skóre: +2 ak sestra je v danej miestnosti, +1 ak lekar je tam, +1 ak pacient je tam
			int bestScore = -1;
			int bestIdx   = 0;
			Sestra? bestSestra = null;
			Lekar?  bestLekar  = null;

			if (useA)
			{
				for (int i = 0; i < MyAgent.MiestnostiAVolne.Count; i++)
				{
					var m = MyAgent.MiestnostiAVolne[i];
					var s = MyAgent.SestryVolne.FirstOrDefault(x =>
						MyAgent.SestraPoloha.TryGetValue(x.Id, out var p) && p == m);
					var l = MyAgent.LekariVolne.FirstOrDefault(x =>
						MyAgent.LekarPoloha.TryGetValue(x.Id, out var p) && p == m);
					int score = (s != null ? 2 : 0) + (l != null ? 1 : 0)
					          + (Sim.PacientJeUzVMiestnosti(msg.PacientId, m) ? 1 : 0);
					if (score > bestScore) { bestScore = score; bestIdx = i; bestSestra = s; bestLekar = l; }
					if (bestScore == 4) break;
				}
				msg.PridelenaMiestnost = MyAgent.MiestnostiAVolne[bestIdx];
				MyAgent.MiestnostiAVolne.RemoveAt(bestIdx);
			}
			else
			{
				for (int i = 0; i < MyAgent.MiestnostiBVolne.Count; i++)
				{
					var m = MyAgent.MiestnostiBVolne[i];
					var s = MyAgent.SestryVolne.FirstOrDefault(x =>
						MyAgent.SestraPoloha.TryGetValue(x.Id, out var p) && p == m);
					var l = MyAgent.LekariVolne.FirstOrDefault(x =>
						MyAgent.LekarPoloha.TryGetValue(x.Id, out var p) && p == m);
					int score = (s != null ? 2 : 0) + (l != null ? 1 : 0)
					          + (Sim.PacientJeUzVMiestnosti(msg.PacientId, m) ? 1 : 0);
					if (score > bestScore) { bestScore = score; bestIdx = i; bestSestra = s; bestLekar = l; }
					if (bestScore == 4) break;
				}
				msg.PridelenaMiestnost = MyAgent.MiestnostiBVolne[bestIdx];
				MyAgent.MiestnostiBVolne.RemoveAt(bestIdx);
			}

			if (bestSestra != null) { msg.PriradenaSestrа = bestSestra; MyAgent.SestryVolne.Remove(bestSestra); }
			if (bestLekar  != null) { msg.PriradenyLekar  = bestLekar;  MyAgent.LekariVolne.Remove(bestLekar); }
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
