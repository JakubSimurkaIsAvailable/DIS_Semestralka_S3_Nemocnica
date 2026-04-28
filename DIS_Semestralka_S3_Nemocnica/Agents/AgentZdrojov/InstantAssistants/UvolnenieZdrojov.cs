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
			switch (message.Code)
			{
				case Mc.UvolnenieZdrojovVstupneVysetrenie:
					MyAgent.VolneSestry++;
					MyAgent.VolneMiestnostiB++;
					break;
				case Mc.UvolnenieZdrojovOsetrenie:
					MyAgent.VolneLekari++;
					MyAgent.VolneSestry++;
					if (msg.PouzilaMiestnostA) MyAgent.VolneMiestnostiA++;
					else                       MyAgent.VolneMiestnostiB++;
					break;
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
