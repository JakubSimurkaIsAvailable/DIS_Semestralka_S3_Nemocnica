using OSPABA;
using Simulation;
using Agents.AgentZdrojov;

namespace Agents.AgentZdrojov.InstantAssistants
{
	//meta! id="124"
	public class UvolnenieZdrojov : OSPABA.Action
	{
		public UvolnenieZdrojov(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		override public void Execute(MessageForm message)
		{
			var msg = (MyMessage)message;
			if (msg.JePresunNaOsetrenie)
			{
				MyAgent.VolneLekari++;
				MyAgent.VolneSestry++;
				if (msg.PouzilaMiestnostA) MyAgent.VolneMiestnostiA++;
				else                       MyAgent.VolneMiestnostiB++;
			}
			else
			{
				MyAgent.VolneSestry++;
				MyAgent.VolneMiestnostiB++;
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
