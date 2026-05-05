using OSPABA;
using Simulation;
using Simulation.Resources;
using Agents.AgentZdrojov.InstantAssistants;
using DIS_Semestralka_S3_Nemocnica.Collectors;

namespace Agents.AgentZdrojov
{
	//meta! id="12"
	public class AgentZdrojov : OSPABA.Agent
	{
		// ── Master pools (identita každého zdroja) ──
		public List<Sestra>    VsestkySestry    { get; } = new();
		public List<Lekar>     VsetciLekari     { get; } = new();
		public List<MiestnostA> VsetkyMiestnostiA { get; } = new();
		public List<MiestnostB> VsetkyMiestnostiB { get; } = new();

		// ── Voľné fronty ──
		public Queue<Sestra>    SestryVolne      { get; } = new();
		public Queue<Lekar>     LekariVolne      { get; } = new();
		public Queue<MiestnostA> MiestnostiAVolne { get; } = new();
		public Queue<MiestnostB> MiestnostiBVolne { get; } = new();

		// ── Computed ints pre spätnú kompatibilitu (Form1, štatistiky) ──
		public int TotalSestry     => VsestkySestry.Count;
		public int TotalLekari     => VsetciLekari.Count;
		public int TotalMiestnostiA => VsetkyMiestnostiA.Count;
		public int TotalMiestnostiB => VsetkyMiestnostiB.Count;
		public int VolneSestry     => SestryVolne.Count;
		public int VolneLekari     => LekariVolne.Count;
		public int VolneMiestnostiA => MiestnostiAVolne.Count;
		public int VolneMiestnostiB => MiestnostiBVolne.Count;

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

			VsestkySestry.Clear();
			for (int i = 0; i < sim.KonfSestry; i++) VsestkySestry.Add(new Sestra(i));
			SestryVolne.Clear();
			foreach (var s in VsestkySestry) SestryVolne.Enqueue(s);

			VsetciLekari.Clear();
			for (int i = 0; i < sim.KonfLekari; i++) VsetciLekari.Add(new Lekar(i));
			LekariVolne.Clear();
			foreach (var l in VsetciLekari) LekariVolne.Enqueue(l);

			VsetkyMiestnostiA.Clear();
			for (int i = 0; i < sim.KonfMiestnostiA; i++) VsetkyMiestnostiA.Add(new MiestnostA(i));
			MiestnostiAVolne.Clear();
			foreach (var m in VsetkyMiestnostiA) MiestnostiAVolne.Enqueue(m);

			VsetkyMiestnostiB.Clear();
			for (int i = 0; i < sim.KonfMiestnostiB; i++) VsetkyMiestnostiB.Add(new MiestnostB(i));
			MiestnostiBVolne.Clear();
			foreach (var m in VsetkyMiestnostiB) MiestnostiBVolne.Enqueue(m);

			ResetLocalStats();
			base.PrepareReplication();
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
