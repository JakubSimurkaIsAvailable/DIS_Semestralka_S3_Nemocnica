using OSPABA;
using Simulation;
using Agents.AgentZdrojov.InstantAssistants;

namespace Agents.AgentZdrojov
{
	//meta! id="12"
	public class AgentZdrojov : OSPABA.Agent
	{
		public int VolneSestry { get; set; } = 3;   // TODO: vstupny parameter
		public int VolneLekari { get; set; } = 2;   // TODO: vstupny parameter
		public int VolneMiestnostiA { get; set; } = 5;
		public int VolneMiestnostiB { get; set; } = 7;

		// klic: (0=sanitka/1=regular, PacientId) → sanitka vzdy pred regular, v ramci skupiny FIFO
		public PriorityQueue<MyMessage, (int, int)> PendingVstupneVysetrenie { get; } = new();
		public PriorityQueue<MyMessage, (int Priorita, int PacientId)> PendingOsetrenie { get; } = new();

		public AgentZdrojov(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			VolneSestry = 3;     // TODO: vstupny parameter
			VolneLekari = 2;     // TODO: vstupny parameter
			VolneMiestnostiA = 5;
			VolneMiestnostiB = 7;
			PendingVstupneVysetrenie.Clear();
			PendingOsetrenie.Clear();
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
