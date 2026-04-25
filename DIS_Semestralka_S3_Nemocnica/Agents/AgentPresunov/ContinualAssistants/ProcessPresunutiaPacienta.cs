using OSPABA;
using Agents.AgentPresunov;
using Simulation;
using OSPRNG;

namespace Agents.AgentPresunov.ContinualAssistants
{
	//meta! id="73"
	public class ProcessPresunutiaPacienta : OSPABA.Process
	{
		private OSPRNG.TriangularRNG _prichodSamostatne = new OSPRNG.TriangularRNG(120, 150, 300);
		private OSPRNG.UniformContinuousRNG _prichodSanitka = new OSPRNG.UniformContinuousRNG(90, 200);
		private OSPRNG.UniformContinuousRNG _odchod = new OSPRNG.UniformContinuousRNG(150, 240);
        public ProcessPresunutiaPacienta(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
		}

		//meta! sender="AgentPresunov", id="74", type="Start"
		public void ProcessStart(MessageForm message)
		{
			MyMessage sprava = (MyMessage)message;
			RNG<double> rng;
			if (sprava.JeOdchod)
			{
				rng = _odchod;
			}
			else if (sprava.PrisielSanitkou)
			{
				rng = _prichodSanitka;
			}
			else
			{
				rng = _prichodSamostatne;
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
		public new AgentPresunov MyAgent
		{
			get
			{
				return (AgentPresunov)base.MyAgent;
			}
		}
	}
}
