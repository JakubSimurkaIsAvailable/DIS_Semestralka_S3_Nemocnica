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

		// ── Voľné zoznamy ──
		public List<Sestra>    SestryVolne      { get; } = new();
		public List<Lekar>     LekariVolne      { get; } = new();
		public List<MiestnostA> MiestnostiAVolne { get; } = new();
		public List<MiestnostB> MiestnostiBVolne { get; } = new();

		// ── Tracking polohy personálu pre minimalizáciu pohybu ──
		// null = personál je v čakacej zóne (idle)
		public Dictionary<int, Miestnost?> SestraPoloha { get; } = new();
		public Dictionary<int, Miestnost?> LekarPoloha  { get; } = new();

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
		public HashSet<int> CakajuciOsetrenieIds { get; } = new();

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

		public WeightedStatisticsCollector LocDlzkaRaduA { get; private set; } = new();
		public WeightedStatisticsCollector LocDlzkaRaduAB { get; private set; } = new();
		public WeightedStatisticsCollector LocDlzkaRaduB { get; private set; } = new();
		public WeightedStatisticsCollector LocDlzkaRaduVV { get; private set; } = new();


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

			//------
			LocDlzkaRaduA = new WeightedStatisticsCollector();
			LocDlzkaRaduAB = new WeightedStatisticsCollector();
			LocDlzkaRaduB = new WeightedStatisticsCollector();
			LocDlzkaRaduVV = new WeightedStatisticsCollector();

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
			SestryVolne.AddRange(VsestkySestry);

			VsetciLekari.Clear();
			for (int i = 0; i < sim.KonfLekari; i++) VsetciLekari.Add(new Lekar(i));
			LekariVolne.Clear();
			LekariVolne.AddRange(VsetciLekari);

			VsetkyMiestnostiA.Clear();
			for (int i = 0; i < sim.KonfMiestnostiA; i++) VsetkyMiestnostiA.Add(new MiestnostA(i));
			MiestnostiAVolne.Clear();
			MiestnostiAVolne.AddRange(VsetkyMiestnostiA);

			VsetkyMiestnostiB.Clear();
			for (int i = 0; i < sim.KonfMiestnostiB; i++) VsetkyMiestnostiB.Add(new MiestnostB(i));
			MiestnostiBVolne.Clear();
			MiestnostiBVolne.AddRange(VsetkyMiestnostiB);

			SestraPoloha.Clear();
			LekarPoloha.Clear();

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
			CakajuciOsetrenieIds.Clear();
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
