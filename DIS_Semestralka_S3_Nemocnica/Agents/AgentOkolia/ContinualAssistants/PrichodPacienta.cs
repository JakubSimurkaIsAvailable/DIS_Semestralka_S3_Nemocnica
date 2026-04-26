using Agents.AgentOkolia;
using DIS_Semestralka_S3_Nemocnica.Generators;
using OSPABA;
using Simulation;

namespace Agents.AgentOkolia.ContinualAssistants
{
	//meta! id="54"
	public class PrichodPacienta : OSPABA.Scheduler
	{
		private ExponencialnyGenerator _exp;
		public PrichodPacienta(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
			_exp = new ExponencialnyGenerator(((MySimulation)mySim).SeedRandom, 1.0 / 575);
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
		}

		//meta! sender="AgentOkolia", id="55", type="Start"
		public void ProcessStart(MessageForm message)
		{
			if (MySim.CurrentTime > 0)
			{
				var notif = (MyMessage)message.CreateCopy();
				notif.PacientId = MyAgent.PocetPacientov++;
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
			return _exp.Generate() - 0.001;
		}
	}
}
