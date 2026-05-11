using OSPABA;
using Simulation;
using Agents.AgentZdrojov;

namespace Agents.AgentZdrojov.InstantAssistants
{
	//meta! id="63"
	public class PriradenieZdrojovPreOsetrenie : OSPABA.Action
	{
		public PriradenieZdrojovPreOsetrenie(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		override public void Execute(MessageForm message)
		{
			var msg = (MyMessage)message;
			// PridelenaMiestnost set by ManagerZdrojov; sestra/lekar may be pre-assigned (MinPohybPersonalu)
			if (msg.PriradenaSestrа == null)
			{
				msg.PriradenaSestrа = MyAgent.SestryVolne[0];
				MyAgent.SestryVolne.RemoveAt(0);
			}
			if (msg.PriradenyLekar == null)
			{
				msg.PriradenyLekar = MyAgent.LekariVolne[0];
				MyAgent.LekariVolne.RemoveAt(0);
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
