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
			if (msg.Priorita <= 2)
			{
				MyAgent.RadA.Enqueue(msg, (msg.Priorita, msg.PacientId));
				MyAgent.RadAItems.Add((msg.PacientId, msg.Priorita));
			}
			else if (msg.Priorita <= 4)
			{
				MyAgent.RadAB.Enqueue(msg, (msg.Priorita, msg.PacientId));
				MyAgent.RadABItems.Add((msg.PacientId, msg.Priorita));
			}
			else
			{
				MyAgent.RadB.Enqueue(msg, (msg.Priorita, msg.PacientId));
				MyAgent.RadBItems.Add((msg.PacientId, msg.Priorita));
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
