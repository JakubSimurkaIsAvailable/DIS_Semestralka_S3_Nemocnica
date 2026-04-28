using Agents.AgentOkolia;
using OSPABA;
using Agents.AgentModelu;
using Agents.AgentUrgentu;
using Agents.AgentPresunov;
using Agents.AgentVstupnehoVysetrenia;
using Agents.AgentZdrojov;
using Agents.AgentOsetrenia;
using DIS_Semestralka_S3_Nemocnica.Collectors;
using System.Collections.Concurrent;

namespace Simulation
{
	public class MySimulation : OSPABA.Simulation
	{
		public Random SeedRandom { get; private set; } = new Random();

		public ConcurrentDictionary<int, PacientInfo> Pacienti { get; } = new();
		public volatile bool Zastavit;
		public volatile int PocetVybavenych;

		// Slowdown params written from UI thread, read from sim thread in PrepareReplication.
		// Plain int reads/writes are atomic on x86-64 so no lock needed.
		public int GuiInterval { get; set; } = 60;   // sim-seconds between scheduler fires
		public int GuiDurationMs { get; set; } = 0;  // real-time sleep in ms (0 = max speed)

		public int KonfSestry { get; set; } = 3;
		public int KonfLekari { get; set; } = 2;
		public int KonfMiestnostiA { get; set; } = 5;
		public int KonfMiestnostiB { get; set; } = 7;

		// Per-replication accumulators (reset each replication)
		public StatisticsCollector LocDobaVV { get; private set; } = new();
		public StatisticsCollector LocDobaOsetrenie { get; private set; } = new();

		// Cross-replication collectors (one mean per replication → CI computation)
		public StatisticsCollector DobaVV { get; } = new();
		public StatisticsCollector DobaOsetrenie { get; } = new();

		public void NastavSeed(int seed) => SeedRandom = new Random(seed);
		public void NastavNahodny() => SeedRandom = new Random();

		public void AktualizujStavPacienta(int id, string stav)
		{
			if (Pacienti.TryGetValue(id, out var info))
				info.Stav = stav;
		}

		public void AktualizujPriorituPacienta(int id, int priorita)
		{
			if (Pacienti.TryGetValue(id, out var info))
				info.Priorita = priorita;
		}

		public void AktualizujMiestnostPacienta(int id, bool pouzilaMiestnostA)
		{
			if (Pacienti.TryGetValue(id, out var info))
				info.PouzilaMiestnostA = pouzilaMiestnostA;
		}

		public MySimulation()
		{
			Init();
			OnReplicationDidFinish(_ =>
			{
				if (LocDobaVV.ValueCounter > 0)        DobaVV.AddValue(LocDobaVV.Average);
				if (LocDobaOsetrenie.ValueCounter > 0) DobaOsetrenie.AddValue(LocDobaOsetrenie.Average);
			});
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Always re-activate slowdown so OnRefreshUI fires (needed for pause support).
			SetSimSpeed(GuiInterval, GuiDurationMs > 0 ? GuiDurationMs / 1000.0 : 0.001);
			if (Zastavit)
			{
				StopSimulation();
				return;
			}
			Pacienti.Clear();
			PocetVybavenych = 0;
			LocDobaVV = new StatisticsCollector();
			LocDobaOsetrenie = new StatisticsCollector();
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			AgentModelu = new AgentModelu(SimId.AgentModelu, this, null);
			AgentOkolia = new AgentOkolia(SimId.AgentOkolia, this, AgentModelu);
			AgentUrgentu = new AgentUrgentu(SimId.AgentUrgentu, this, AgentModelu);
			AgentPresunov = new AgentPresunov(SimId.AgentPresunov, this, AgentUrgentu);
			AgentVstupnehoVysetrenia = new AgentVstupnehoVysetrenia(SimId.AgentVstupnehoVysetrenia, this, AgentUrgentu);
			AgentOsetrenia = new AgentOsetrenia(SimId.AgentOsetrenia, this, AgentUrgentu);
			AgentZdrojov = new AgentZdrojov(SimId.AgentZdrojov, this, AgentUrgentu);
		}
		public AgentModelu AgentModelu
		{ get; set; }
		public AgentOkolia AgentOkolia
		{ get; set; }
		public AgentUrgentu AgentUrgentu
		{ get; set; }
		public AgentPresunov AgentPresunov
		{ get; set; }
		public AgentVstupnehoVysetrenia AgentVstupnehoVysetrenia
		{ get; set; }
		public AgentOsetrenia AgentOsetrenia
		{ get; set; }
		public AgentZdrojov AgentZdrojov
		{ get; set; }
		//meta! tag="end"
	}
}