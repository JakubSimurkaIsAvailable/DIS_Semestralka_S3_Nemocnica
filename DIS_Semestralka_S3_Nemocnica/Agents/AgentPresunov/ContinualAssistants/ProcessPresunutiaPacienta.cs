using DIS_Semestralka_S3_Nemocnica.Generators;
using DIS_Semestralka_S3_Nemocnica.Generators.Components;
using OSPABA;
using Agents.AgentPresunov;
using Simulation;

namespace Agents.AgentPresunov.ContinualAssistants
{
	//meta! id="73"
	public class ProcessPresunutiaPacienta : OSPABA.Process
	{
		private TrojuholnikovyGenerator _prichodSamostatne;
		private RozdelenieSpojite _prichodSanitka;
		private RozdelenieSpojite _odchod;

        public ProcessPresunutiaPacienta(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
			var seed = ((MySimulation)mySim).SeedRandom;
			_prichodSamostatne = new TrojuholnikovyGenerator(seed, 120, 150, 300);
			_prichodSanitka = new RozdelenieSpojite(seed, 90, 200);
			_odchod = new RozdelenieSpojite(seed, 150, 240);
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
		}

		//meta! sender="AgentPresunov", id="74", type="Start"
		public void ProcessStart(MessageForm message)
		{
			var msg = (MyMessage)message;
			double cas = msg.JeOdchod ? _odchod.Generate()
				: msg.PrisielSanitkou ? _prichodSanitka.Generate()
				: _prichodSamostatne.Generate();
			message.Code = Mc.PresunutiePacientaSkoncilo;
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

			case Mc.PresunutiePacientaSkoncilo:
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
