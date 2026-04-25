using OSPABA;
using Agents.AgentUrgentu;
using Simulation;

namespace Agents.AgentUrgentu.InstantAssistants
{
	//meta! id="96"
	public class ZaradenieDoRaduOsetrenie : OSPABA.Action
	{
		public ZaradenieDoRaduOsetrenie(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		override public void Execute(MessageForm message)
		{
		}
		public new AgentUrgentu MyAgent
		{
			get
			{
				return (AgentUrgentu)base.MyAgent;
			}
		}
	}
}