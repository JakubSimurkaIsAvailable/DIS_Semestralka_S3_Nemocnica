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
