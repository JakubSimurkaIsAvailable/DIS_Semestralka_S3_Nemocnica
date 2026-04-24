using OSPABA;
using Simulation;
using Agents.AgentVstupnehoVysetrenia;

namespace Agents.AgentVstupnehoVysetrenia.InstantAssistants
{
	//meta! id="66"
	public class PriradeniePriority : OSPABA.Action
	{
		public PriradeniePriority(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		override public void Execute(MessageForm message)
		{
		}
		public new AgentVstupnehoVysetrenia MyAgent
		{
			get
			{
				return (AgentVstupnehoVysetrenia)base.MyAgent;
			}
		}
	}
}
