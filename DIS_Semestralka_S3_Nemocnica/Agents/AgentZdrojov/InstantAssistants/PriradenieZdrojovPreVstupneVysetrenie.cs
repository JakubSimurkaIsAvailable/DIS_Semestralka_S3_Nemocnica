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
			// PridelenaMiestnost set by ManagerZdrojov; PriradenaSestrа may be pre-assigned (MinPohybPersonalu)
			if (msg.PriradenaSestrа == null)
			{
				msg.PriradenaSestrа = MyAgent.SestryVolne[0];
				MyAgent.SestryVolne.RemoveAt(0);
			}
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
