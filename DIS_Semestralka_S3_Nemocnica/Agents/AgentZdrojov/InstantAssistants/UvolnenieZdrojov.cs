using OSPABA;
using Simulation;
using Simulation.Resources;
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
				if (msg.PriradenaSestrа != null)    MyAgent.SestryVolne.Enqueue(msg.PriradenaSestrа);
				if (msg.PriradenyLekar  != null)    MyAgent.LekariVolne.Enqueue(msg.PriradenyLekar);
				if      (msg.PridelenaMiestnost is MiestnostA ma) MyAgent.MiestnostiAVolne.Enqueue(ma);
				else if (msg.PridelenaMiestnost is MiestnostB mb) MyAgent.MiestnostiBVolne.Enqueue(mb);
			}
			else
			{
				if (msg.PriradenaSestrа != null)    MyAgent.SestryVolne.Enqueue(msg.PriradenaSestrа);
				if (msg.PridelenaMiestnost is MiestnostB mb)      MyAgent.MiestnostiBVolne.Enqueue(mb);
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
