using DIS_Semestralka_S3_Nemocnica.Generators;
using DIS_Semestralka_S3_Nemocnica.Generators.Components;
using OSPABA;
using Simulation;
using Agents.AgentOsetrenia;

namespace Agents.AgentOsetrenia.ContinualAssistants
{
	//meta! id="40"
	public class ProcessOsetrenie : OSPABA.Process
	{
		// TODO: doplnit spravne rozdelenia podla priority pacienta
		private SpojityEmpirickyGenerator _samRNG;
		private RozdelenieSpojite _sanitkaRNG;
		public ProcessOsetrenie(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
			var seed = ((MySimulation)mySim).SeedRandom;
			_samRNG = new SpojityEmpirickyGenerator(seed,
				new List<double> { 10 * 60, 12 * 60, 14 * 60 },
				new List<double> { 12 * 60, 14 * 60, 18 * 60 },
				new List<double> { 0.1, 0.6, 0.3 });
			_sanitkaRNG = new RozdelenieSpojite(seed, 15 * 60, 30 * 60);
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
		}

        //meta! sender="AgentOsetrenia", id="41", type="Start"
        public void ProcessStart(MessageForm message)
		{
			var msg = (MyMessage)message;
			double cas = msg.PrisielSanitkou ? _sanitkaRNG.Generate() : _samRNG.Generate();
			message.Code = Mc.OsetrenieSkoncilo;
			Hold(cas, message);
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

			case Mc.OsetrenieSkoncilo:
				AssistantFinished(message);
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
