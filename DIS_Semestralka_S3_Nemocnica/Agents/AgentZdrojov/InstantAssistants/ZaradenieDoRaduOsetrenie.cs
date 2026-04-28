using OSPABA;
using Simulation;
using Agents.AgentZdrojov;

namespace Agents.AgentZdrojov.InstantAssistants
{
	//meta! id="133"
	public class ZaradenieDoRaduOsetrenie : OSPABA.Action
	{
		public ZaradenieDoRaduOsetrenie(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		override public void Execute(MessageForm message)
		{
			var msg = (MyMessage)message;
			msg.CasVstupuDoRadu = MySim.CurrentTime;
			MyAgent.RadOsetrenie.Enqueue(msg, (msg.Priorita, msg.PacientId));
			MyAgent.RadOsetreniaItems.Add((msg.PacientId, msg.Priorita));
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
