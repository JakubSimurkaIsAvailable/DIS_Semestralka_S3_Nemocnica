using OSPABA;
using Simulation;
using Agents.AgentZdrojov.InstantAssistants;

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

		public AgentZdrojov(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			var sim = (MySimulation)MySim;
			TotalSestry = sim.KonfSestry;
			TotalLekari = sim.KonfLekari;
			TotalMiestnostiA = sim.KonfMiestnostiA;
			TotalMiestnostiB = sim.KonfMiestnostiB;
			VolneSestry = TotalSestry;
			VolneLekari = TotalLekari;
			VolneMiestnostiA = TotalMiestnostiA;
			VolneMiestnostiB = TotalMiestnostiB;
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
