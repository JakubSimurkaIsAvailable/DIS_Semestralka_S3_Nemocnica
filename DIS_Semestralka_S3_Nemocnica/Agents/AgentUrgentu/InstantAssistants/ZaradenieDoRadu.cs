using OSPABA;
using Agents.AgentUrgentu;
using Simulation;

namespace Agents.AgentUrgentu.InstantAssistants
{
	//meta! id="80"
	public class ZaradenieDoRadu : OSPABA.Action
	{
		public ZaradenieDoRadu(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
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
