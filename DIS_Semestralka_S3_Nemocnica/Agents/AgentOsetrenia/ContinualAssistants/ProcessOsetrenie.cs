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

		// TODO: doplnit spravne rozdelenia podla priority pacienta
        private static readonly OSPRNG.EmpiricPair<double>[] _sam = {
            new OSPRNG.EmpiricPair<double>(new OSPRNG.UniformContinuousRNG(10 * 60, 12 * 60), 0.1),
            new OSPRNG.EmpiricPair<double>(new OSPRNG.UniformContinuousRNG(12 * 60, 14 * 60), 0.6),
			new OSPRNG.EmpiricPair<double>(new OSPRNG.UniformContinuousRNG(14 * 60, 18 * 60), 0.3)
        };
		private static readonly OSPRNG.EmpiricRNG<double> _samRNG = new OSPRNG.EmpiricRNG<double>(_sam);
        private static readonly OSPRNG.UniformContinuousRNG _sanitkaRNG = new OSPRNG.UniformContinuousRNG(15 * 60, 30 * 60);
		private HashSet<int> _active = new HashSet<int>();

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			_active.Clear();
		}

        //meta! sender="AgentOsetrenia", id="41", type="Start"
        public void ProcessStart(MessageForm message)
		{
			var msg = (MyMessage)message;
			if (_active.Contains(msg.PacientId))
			{
				_active.Remove(msg.PacientId);
				AssistantFinished(message);
				return;
			}
			_active.Add(msg.PacientId);
			OSPRNG.RNG<double> rng = msg.PrisielSanitkou ? (OSPRNG.RNG<double>)_sanitkaRNG : _samRNG;
			Hold(rng.Sample(), message);
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
		public new AgentOsetrenia MyAgent
		{
			get
			{
				return (AgentOsetrenia)base.MyAgent;
			}
		}
	}
}
