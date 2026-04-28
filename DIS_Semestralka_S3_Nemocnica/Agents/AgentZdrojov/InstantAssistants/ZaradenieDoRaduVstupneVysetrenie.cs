using OSPABA;
using Simulation;
using Agents.AgentZdrojov;

namespace Agents.AgentZdrojov.InstantAssistants
{
	//meta! id="131"
	public class ZaradenieDoRaduVstupneVysetrenie : OSPABA.Action
	{
		public ZaradenieDoRaduVstupneVysetrenie(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		override public void Execute(MessageForm message)
		{
			var msg = (MyMessage)message;
			msg.CasVstupuDoRadu = MySim.CurrentTime;
			int skupinaKey = msg.PrisielSanitkou ? 0 : 1;
			MyAgent.RadVV.Enqueue(msg, (skupinaKey, msg.PacientId));
			MyAgent.RadVVIds.Add(msg.PacientId);
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
