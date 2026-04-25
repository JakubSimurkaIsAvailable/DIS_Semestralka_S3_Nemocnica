using OSPABA;
using Simulation;
using Agents.AgentOsetrenia;

namespace Agents.AgentOsetrenia.ContinualAssistants
{
	//meta! id="40"
	public class ProcessOsetrenie : OSPABA.Process
	{
		public ProcessOsetrenie(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
		}

        // TODO: doplnit spravne rozdelenia podla priority pacienta
        private static readonly OSPRNG.EmpiricPair<double>[] _sam = {
            new OSPRNG.EmpiricPair<double>(new OSPRNG.UniformContinuousRNG(10 * 60, 12 * 60), 0.1),
            new OSPRNG.EmpiricPair<double>(new OSPRNG.UniformContinuousRNG(12 * 60, 14 * 60), 0.6),
			new OSPRNG.EmpiricPair<double>(new OSPRNG.UniformContinuousRNG(14 * 60, 18 * 60), 0.3)
        };
		private static readonly OSPRNG.EmpiricRNG<double> _samRNG = new OSPRNG.EmpiricRNG<double>(_sam);
        private static readonly OSPRNG.UniformContinuousRNG _sanitkaRNG = new OSPRNG.UniformContinuousRNG(15 * 60, 30 * 60);

        //meta! sender="AgentOsetrenia", id="41", type="Start"
        public void ProcessStart(MessageForm message)
		{
			var msg = (MyMessage)message;
			OSPRNG.RNG<double> rng;
            if(msg.PrisielSanitkou == true)
			{
				rng = _sanitkaRNG;
			}
			else
			{
				rng = _samRNG;
            }
            Hold(rng.Sample(), message);
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
		public new AgentOsetrenia MyAgent
		{
			get
			{
				return (AgentOsetrenia)base.MyAgent;
			}
		}
	}
}
