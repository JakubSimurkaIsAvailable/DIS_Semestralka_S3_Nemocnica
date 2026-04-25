using OSPABA;
using Simulation;
using Agents.AgentVstupnehoVysetrenia;
using OSPRNG;

namespace Agents.AgentVstupnehoVysetrenia.ContinualAssistants
{
	//meta! id="38"
	public class ProcessVstupneVysetrenie : OSPABA.Process
	{
        //prisiel sam dlzka trvania vstupneho vysetrenia
        private static readonly OSPRNG.EmpiricPair<double>[] _sam = {
			new OSPRNG.EmpiricPair<double>(new OSPRNG.UniformContinuousRNG(3, 5), 0.6),
			new OSPRNG.EmpiricPair<double>(new OSPRNG.UniformContinuousRNG(5, 9), 0.4)
		};

        private OSPRNG.EmpiricRNG<double> empiricRNG = new OSPRNG.EmpiricRNG<double>(_sam);
		//--------------------------

		//prisiel sanitkou dlzka trvania vstupneho vysetrenia
		private static readonly OSPRNG.UniformDiscreteRNG discreteRNG = new OSPRNG.UniformDiscreteRNG(4, 8);
        public ProcessVstupneVysetrenie(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
		}

		//meta! sender="AgentVstupnehoVysetrenia", id="39", type="Start"
		public void ProcessStart(MessageForm message)
		{
			MyMessage sprava = (MyMessage)message;
			double trvanieVstupnehoVysetrenia;
            if (sprava.PrisielSanitkou)
			{
				trvanieVstupnehoVysetrenia = discreteRNG.Sample();
            } else
			{
				trvanieVstupnehoVysetrenia = empiricRNG.Sample();
            }
			Hold(trvanieVstupnehoVysetrenia, message);
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
			case Mc.Finish:
				AssistantFinished(message);
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
		public new AgentVstupnehoVysetrenia MyAgent
		{
			get
			{
				return (AgentVstupnehoVysetrenia)base.MyAgent;
			}
		}
	}
}
