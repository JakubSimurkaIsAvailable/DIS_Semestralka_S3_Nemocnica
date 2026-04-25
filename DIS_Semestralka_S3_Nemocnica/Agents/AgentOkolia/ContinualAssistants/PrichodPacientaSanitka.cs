using Agents.AgentOkolia;
using OSPABA;
using Simulation;

namespace Agents.AgentOkolia.ContinualAssistants
{
	//meta! id="56"
	public class PrichodPacientaSanitka : OSPABA.Scheduler
	{
		private OSPRNG.ErlangRNG erlang = new OSPRNG.ErlangRNG(67.5, 4.37 * 4.37);
        public PrichodPacientaSanitka(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
		}

		//meta! sender="AgentOkolia", id="57", type="Start"
		public void ProcessStart(MessageForm message)
		{
            Hold(VygenerujCas(), message);
        }

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
                case Mc.Finish:
                    var sprava = (MyMessage)message;
                    sprava.PacientId = MyAgent.PocetPacientov;
                    AssistantFinished(sprava);

                    MyMessage copy = (MyMessage)sprava.CreateCopy();
                    Hold(VygenerujCas(), copy);
                    break;
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
			return 56 + erlang.Sample();
        }
    }
}
