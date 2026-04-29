using OSPABA;
using Simulation;
using Agents.AgentZdrojov.InstantAssistants;

namespace Agents.AgentZdrojov
{
	//meta! id="12"
	public class AgentZdrojov : OSPABA.Agent
	{
		public AgentZdrojov(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
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
