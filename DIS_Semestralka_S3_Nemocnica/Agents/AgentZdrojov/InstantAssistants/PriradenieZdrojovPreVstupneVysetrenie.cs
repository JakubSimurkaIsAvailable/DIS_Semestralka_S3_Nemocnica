using OSPABA;
using Simulation;
using Agents.AgentZdrojov;

namespace Agents.AgentZdrojov.InstantAssistants
{
	//meta! id="61"
	public class PriradenieZdrojovPreVstupneVysetrenie : OSPABA.Action
	{
		public PriradenieZdrojovPreVstupneVysetrenie(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		override public void Execute(MessageForm message)
		{
			var msg = (MyMessage)message;
			// PridelenaMiestnost (MiestnostB) already set by ManagerZdrojov before calling Execute
			msg.PriradenaSestrа = MyAgent.SestryVolne.Dequeue();
		}
		public new AgentZdrojov MyAgent
		{
			get
			{
				return (AgentZdrojov)base.MyAgent;
			}
		}
	}
}
