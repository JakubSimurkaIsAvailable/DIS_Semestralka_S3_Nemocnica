using Agents.AgentPresunov;
using OSPABA;
using Simulation;
using Simulation.Resources;

namespace Agents.AgentPresunov.ContinualAssistants
{
	//meta! id="75"
	public class ProcessPresunitiaPersonalu : OSPABA.Process
	{

        public ProcessPresunitiaPersonalu(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
        }

		override public void PrepareReplication()
		{
			base.PrepareReplication();
		}

        //meta! sender="AgentPresunov", id="76", type="Start"
        public void ProcessStart(MessageForm message)
		{
			var msg = (MyMessage)message;
			var sim = (MySimulation)MySim;
			double cas;
			if (msg.JePresunNaOsetrenie)
			{
				double tSestra  = CasPresunutia(msg.PriradenaSestrа!, msg.PridelenaMiestnost!, sim);
				double tLekar   = CasPresunutia(msg.PriradenyLekar!,  msg.PridelenaMiestnost!, sim);
				double tPacient = sim.PacientJeUzVMiestnosti(msg.PacientId, msg.PridelenaMiestnost!) ? 0 : MyAgent.GenerujCasPresunuPersonalu();
				cas = Math.Max(Math.Max(tSestra, tLekar), tPacient);
				sim.AnimStaffPohybDoMiestnosti(msg.PacientId, tSestra, tLekar);
				sim.AnimPacientPohybDoOsetrenia(msg.PacientId, tPacient);
			}
			else
			{
				cas = CasPresunutia(msg.PriradenaSestrа!, msg.PridelenaMiestnost!, sim);
				sim.AnimSestryPohybDoMiestnosti(msg.PacientId, cas);
			}
			message.Code = Mc.PresunutiePersonaluSkoncilo;
			Hold(cas, message);
		}

		private double CasPresunutia(Sestra sestra, Miestnost miestnost, MySimulation sim)
		{
			if (sim.SestraJeUzVMiestnosti(sestra, miestnost)) return 0;
			return sim.SestraJeVAmbulancii(sestra) ? MyAgent.GenerujCasPresunuPersonalu() : MyAgent.GenerujCasPresunuPersonaluZCakarni();
		}

		private double CasPresunutia(Lekar lekar, Miestnost miestnost, MySimulation sim)
		{
			if (sim.LekarJeUzVMiestnosti(lekar, miestnost)) return 0;
			return sim.LekarJeVAmbulancii(lekar) ? MyAgent.GenerujCasPresunuPersonalu() : MyAgent.GenerujCasPresunuPersonaluZCakarni();
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
			}
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		override public void ProcessMessage(MessageForm message)
		{
			switch (message.Code)
			{
			case Mc.Start:
				ProcessStart(message);
			break;

			case Mc.PresunutiePersonaluSkoncilo:
				AssistantFinished(message);
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new AgentPresunov MyAgent
		{
			get
			{
				return (AgentPresunov)base.MyAgent;
			}
		}
	}
}
