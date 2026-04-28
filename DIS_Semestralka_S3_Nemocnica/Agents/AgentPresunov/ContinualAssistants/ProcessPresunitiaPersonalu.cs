using DIS_Semestralka_S3_Nemocnica.Generators;
using OSPABA;
using Agents.AgentPresunov;
using Simulation;

namespace Agents.AgentPresunov.ContinualAssistants
{
	//meta! id="75"
	public class ProcessPresunitiaPersonalu : OSPABA.Process
	{
		// TODO: doplnit spravne rozdelenie pre cas presunu personalu
		private TrojuholnikovyGenerator _presunPersonalu;
		public ProcessPresunitiaPersonalu(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
			_presunPersonalu = new TrojuholnikovyGenerator(((MySimulation)mySim).SeedRandom, 15, 20, 45);
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
		}

        //meta! sender="AgentPresunov", id="76", type="Start"
        public void ProcessStart(MessageForm message)
		{
			var msg = (MyMessage)message;
			double cas = msg.JePresunNaOsetrenie
				? Math.Max(_presunPersonalu.Generate(), _presunPersonalu.Generate())
				: _presunPersonalu.Generate();
			message.Code = Mc.PresunutiePersonaluSkoncilo;
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

			case Mc.PresunutiePersonaluSkoncilo:
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
