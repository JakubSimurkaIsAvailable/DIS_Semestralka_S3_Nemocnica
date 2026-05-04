using Agents.AgentOkolia;
using DIS_Semestralka_S3_Nemocnica.Generators;
using OSPABA;
using Simulation;

namespace Agents.AgentOkolia.ContinualAssistants
{
	//meta! id="56"
	public class PrichodPacientaSanitka : OSPABA.Scheduler
	{
		private GammaGenerator _gamma;

        public PrichodPacientaSanitka(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
			_gamma = new GammaGenerator(((MySimulation)mySim).SeedRandom, 67.5, 4.37);
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
		}

		//meta! sender="AgentOkolia", id="57", type="Start"
		public void ProcessStart(MessageForm message)
		{
			if (MySim.CurrentTime > 0)
			{
				var notif = (MyMessage)message.CreateCopy();
				notif.PacientId = MyAgent.PocetPacientov++;
				notif.PrisielSanitkou = true;
				AssistantFinished(notif);
			}
			Hold(VygenerujCas(), message);

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

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new AgentOkolia MyAgent
		{
			get
			{
				return (AgentOkolia)base.MyAgent;
			}
		}

		private double VygenerujCas()
		{
			return 56 + _gamma.Generate();
        }
    }
}
