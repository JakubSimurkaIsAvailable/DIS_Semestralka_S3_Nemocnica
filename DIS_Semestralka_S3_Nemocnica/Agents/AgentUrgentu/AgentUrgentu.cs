using OSPABA;
using Simulation;
using Agents.AgentUrgentu.InstantAssistants;

namespace Agents.AgentUrgentu
{
	//meta! id="3"
	public class AgentUrgentu : OSPABA.Agent
	{
		// sanitka=0, pešo=1 → sanitka má vždy prednosť, v rámci skupiny FIFO
		public PriorityQueue<MyMessage, (int, int)> RadVV { get; } = new();
		public PriorityQueue<MyMessage, (int Priorita, int PacientId)> RadOsetrenie { get; } = new();

		public List<int> RadVVIds { get; } = new();
		public List<(int Id, int Priorita)> RadOsetreniaItems { get; } = new();

		public AgentUrgentu(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			RadVV.Clear();
			RadOsetrenie.Clear();
			RadVVIds.Clear();
			RadOsetreniaItems.Clear();
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new ManagerUrgentu(SimId.ManagerUrgentu, MySim, this);
			new ZaradenieDoRaduVstupneVysetrenie(SimId.ZaradenieDoRaduVstupneVysetrenie, MySim, this);
			new ZaradenieDoRaduOsetrenie(SimId.ZaradenieDoRaduOsetrenie, MySim, this);
            AddOwnMessage(Mc.VysetreniePacienta);
			AddOwnMessage(Mc.VykonanieVstupnehoVysetrenia);
			AddOwnMessage(Mc.PresunPacienta);
			AddOwnMessage(Mc.PridelenieZdrojovVstupneVysetrenie);
			AddOwnMessage(Mc.VykonanieOsetrenia);
			AddOwnMessage(Mc.PresunPersonalu);
			AddOwnMessage(Mc.PridelenieZdrojovOsetrenie);
		}
		//meta! tag="end"
	}
}
