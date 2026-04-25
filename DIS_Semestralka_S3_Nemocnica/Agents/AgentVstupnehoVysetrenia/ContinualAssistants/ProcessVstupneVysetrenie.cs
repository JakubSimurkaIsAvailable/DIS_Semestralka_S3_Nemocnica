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
			new OSPRNG.EmpiricPair<double>(new OSPRNG.UniformContinuousRNG(3 * 60, 5 * 60), 0.6),
			new OSPRNG.EmpiricPair<double>(new OSPRNG.UniformContinuousRNG(5 * 60, 9 * 60), 0.4)
		};

        private OSPRNG.EmpiricRNG<double> empiricRNG = new OSPRNG.EmpiricRNG<double>(_sam);
		//--------------------------

		//prisiel sanitkou dlzka trvania vstupneho vysetrenia
		private static readonly OSPRNG.UniformDiscreteRNG discreteRNG = new OSPRNG.UniformDiscreteRNG(4, 8);
        public ProcessVstupneVysetrenie(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		private HashSet<int> _active = new HashSet<int>();

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			_active.Clear();
		}

		//meta! sender="AgentVstupnehoVysetrenia", id="39", type="Start"
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
			double trvanie = msg.PrisielSanitkou
				? discreteRNG.Sample()
				: empiricRNG.Sample();
			Hold(trvanie, message);
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
		public new AgentVstupnehoVysetrenia MyAgent
		{
			get
			{
				return (AgentVstupnehoVysetrenia)base.MyAgent;
			}
		}
	}
}
