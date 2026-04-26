using OSPABA;
using Simulation;
using Agents.AgentZdrojov.InstantAssistants;

namespace Agents.AgentZdrojov
{
	//meta! id="12"
	public class AgentZdrojov : OSPABA.Agent
	{
		public const int TotalSestry = 3;
		public const int TotalLekari = 2;
		public const int TotalMiestnostiA = 5;
		public const int TotalMiestnostiB = 7;

		public int VolneSestry { get; set; } = TotalSestry;
		public int VolneLekari { get; set; } = TotalLekari;
		public int VolneMiestnostiA { get; set; } = TotalMiestnostiA;
		public int VolneMiestnostiB { get; set; } = TotalMiestnostiB;

		// klic: (0=sanitka/1=regular, PacientId) → sanitka vzdy pred regular, v ramci skupiny FIFO
		public PriorityQueue<MyMessage, (int, int)> PendingVstupneVysetrenie { get; } = new();
		public PriorityQueue<MyMessage, (int Priorita, int PacientId)> PendingOsetrenie { get; } = new();

		public List<int> RadVVIds { get; } = new();
		public List<(int Id, int Priorita)> RadOsetreniaItems { get; } = new();

		public AgentZdrojov(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			VolneSestry = TotalSestry;
			VolneLekari = TotalLekari;
			VolneMiestnostiA = TotalMiestnostiA;
			VolneMiestnostiB = TotalMiestnostiB;
			PendingVstupneVysetrenie.Clear();
			PendingOsetrenie.Clear();
			RadVVIds.Clear();
			RadOsetreniaItems.Clear();
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new ManagerZdrojov(SimId.ManagerZdrojov, MySim, this);
			new PriradenieZdrojovPreOsetrenie(SimId.PriradenieZdrojovPreOsetrenie, MySim, this);
			new PriradenieZdrojovPreVstupneVysetrenie(SimId.PriradenieZdrojovPreVstupneVysetrenie, MySim, this);
			AddOwnMessage(Mc.PridelenieZdrojovVstupneVysetrenie);
			AddOwnMessage(Mc.PridelenieZdrojovOsetrenie);
			AddOwnMessage(Mc.UvolnenieZdrojovVstupneVysetrenie);
			AddOwnMessage(Mc.UvolnenieZdrojovOsetrenie);
		}
		//meta! tag="end"
	}
}
