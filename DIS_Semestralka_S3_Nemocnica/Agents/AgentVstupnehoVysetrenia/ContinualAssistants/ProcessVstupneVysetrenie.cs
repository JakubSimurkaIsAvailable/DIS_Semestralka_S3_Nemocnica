using DIS_Semestralka_S3_Nemocnica.Generators;
using DIS_Semestralka_S3_Nemocnica.Generators.Components;
using OSPABA;
using Simulation;
using Agents.AgentVstupnehoVysetrenia;

namespace Agents.AgentVstupnehoVysetrenia.ContinualAssistants
{
	//meta! id="38"
	public class ProcessVstupneVysetrenie : OSPABA.Process
	{
		private SpojityEmpirickyGenerator empiricRNG;
		private RozdelenieDiskretne discreteRNG;

        public ProcessVstupneVysetrenie(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
			var seed = ((MySimulation)mySim).SeedRandom;
			empiricRNG = new SpojityEmpirickyGenerator(seed,
				new List<double> { 3 * 60, 5 * 60 },
				new List<double> { 5 * 60, 9 * 60 },
				new List<double> { 0.6, 0.4 });
			discreteRNG = new RozdelenieDiskretne(seed, 4, 8);
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
				? discreteRNG.Generate()
				: empiricRNG.Generate();
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
