using OSPABA;
using Simulation;
using Agents.AgentZdrojov.InstantAssistants;
using DIS_Semestralka_S3_Nemocnica.Collectors;

namespace Agents.AgentZdrojov
{
	//meta! id="12"
	public class AgentZdrojov : OSPABA.Agent
	{
		public int TotalSestry { get; set; } = 3;
		public int TotalLekari { get; set; } = 2;
		public int TotalMiestnostiA { get; set; } = 5;
		public int TotalMiestnostiB { get; set; } = 7;

		public int VolneSestry { get; set; }
		public int VolneLekari { get; set; }
		public int VolneMiestnostiA { get; set; }
		public int VolneMiestnostiB { get; set; }

		public PriorityQueue<MyMessage, (int, int)> RadVV { get; } = new();
		public PriorityQueue<MyMessage, (int Priorita, int PacientId)> RadA  { get; } = new();
		public PriorityQueue<MyMessage, (int Priorita, int PacientId)> RadAB { get; } = new();
		public PriorityQueue<MyMessage, (int Priorita, int PacientId)> RadB  { get; } = new();
		public List<int> RadVVIds { get; } = new();
		public List<(int Id, int Priorita)> RadAItems  { get; } = new();
		public List<(int Id, int Priorita)> RadABItems { get; } = new();
		public List<(int Id, int Priorita)> RadBItems  { get; } = new();

		// ── Lokálne štatistiky (reset na začiatku každej replikácie) ──
		public StatisticsCollector LocDobaVV             { get; private set; } = new();
		public StatisticsCollector LocDobaVVPeso         { get; private set; } = new();
		public StatisticsCollector LocDobaVVSanitka      { get; private set; } = new();
		public StatisticsCollector LocDobaOsetrenie      { get; private set; } = new();
		public StatisticsCollector LocDobaOsetrenieA     { get; private set; } = new();
		public StatisticsCollector LocDobaOsetrenieAB    { get; private set; } = new();
		public StatisticsCollector LocDobaOsetrenieB     { get; private set; } = new();
		public StatisticsCollector LocDobaPrichodDoOsetrenia        { get; private set; } = new();
		public StatisticsCollector LocDobaPrichodDoOsetreniaPeso    { get; private set; } = new();
		public StatisticsCollector LocDobaPrichodDoOsetreniaSanitka { get; private set; } = new();
		public WeightedStatisticsCollector LocVytazenostLekari      { get; private set; } = new();
		public WeightedStatisticsCollector LocVytazenostSestry      { get; private set; } = new();
		public WeightedStatisticsCollector LocVytazenostMiestnostiA { get; private set; } = new();
		public WeightedStatisticsCollector LocVytazenostMiestnostiB { get; private set; } = new();

		public void ResetLocalStats()
		{
			LocDobaVV             = new StatisticsCollector();
			LocDobaVVPeso         = new StatisticsCollector();
			LocDobaVVSanitka      = new StatisticsCollector();
			LocDobaOsetrenie      = new StatisticsCollector();
			LocDobaOsetrenieA     = new StatisticsCollector();
			LocDobaOsetrenieAB    = new StatisticsCollector();
			LocDobaOsetrenieB     = new StatisticsCollector();
			LocDobaPrichodDoOsetrenia        = new StatisticsCollector();
			LocDobaPrichodDoOsetreniaPeso    = new StatisticsCollector();
			LocDobaPrichodDoOsetreniaSanitka = new StatisticsCollector();
			LocVytazenostLekari      = new WeightedStatisticsCollector();
			LocVytazenostSestry      = new WeightedStatisticsCollector();
			LocVytazenostMiestnostiA = new WeightedStatisticsCollector();
			LocVytazenostMiestnostiB = new WeightedStatisticsCollector();
		}

		public void ResetLocalStatsAtWarmupEnd(double currentTime)
		{
			ResetLocalStats();
			if (TotalLekari > 0)
				LocVytazenostLekari.AddWeightedValue((double)(TotalLekari - VolneLekari) / TotalLekari, currentTime);
			if (TotalSestry > 0)
				LocVytazenostSestry.AddWeightedValue((double)(TotalSestry - VolneSestry) / TotalSestry, currentTime);
			if (TotalMiestnostiA > 0)
				LocVytazenostMiestnostiA.AddWeightedValue((double)(TotalMiestnostiA - VolneMiestnostiA) / TotalMiestnostiA, currentTime);
			if (TotalMiestnostiB > 0)
				LocVytazenostMiestnostiB.AddWeightedValue((double)(TotalMiestnostiB - VolneMiestnostiB) / TotalMiestnostiB, currentTime);
		}

		public AgentZdrojov(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
		}

		override public void PrepareReplication()
		{
			var sim = (MySimulation)MySim;
			TotalSestry = sim.KonfSestry;
			TotalLekari = sim.KonfLekari;
			TotalMiestnostiA = sim.KonfMiestnostiA;
			TotalMiestnostiB = sim.KonfMiestnostiB;
			VolneSestry = TotalSestry;
			VolneLekari = TotalLekari;
			VolneMiestnostiA = TotalMiestnostiA;
			VolneMiestnostiB = TotalMiestnostiB;
			ResetLocalStats();
			base.PrepareReplication(); // ManagerZdrojov.ZaznamVytazenosti() zapíše využitie=0 pri t=0 ✓
			RadVV.Clear();
			RadA.Clear();
			RadAB.Clear();
			RadB.Clear();
			RadVVIds.Clear();
			RadAItems.Clear();
			RadABItems.Clear();
			RadBItems.Clear();
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
            new ManagerZdrojov(SimId.ManagerZdrojov, MySim, this);
            new ZaradenieDoRaduVstupneVysetrenie(SimId.ZaradenieDoRaduVstupneVysetrenie, MySim, this);
            new UvolnenieZdrojov(SimId.UvolnenieZdrojov, MySim, this);
            new ZaradenieDoRaduOsetrenie(SimId.ZaradenieDoRaduOsetrenie, MySim, this);
            new PriradenieZdrojovPreOsetrenie(SimId.PriradenieZdrojovPreOsetrenie, MySim, this);
            new PriradenieZdrojovPreVstupneVysetrenie(SimId.PriradenieZdrojovPreVstupneVysetrenie, MySim, this);
            AddOwnMessage(Mc.UvolnenieAmbulancie);
            AddOwnMessage(Mc.ZaradenieDoRaduOsetrenie);
            AddOwnMessage(Mc.ZaradenieDoRaduVV);
        }
		//meta! tag="end"
	}
}
