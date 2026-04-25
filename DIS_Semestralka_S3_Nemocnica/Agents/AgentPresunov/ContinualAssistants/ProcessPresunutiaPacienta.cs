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

		private HashSet<int> _active = new HashSet<int>();

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			_active.Clear();
		}

		//meta! sender="AgentPresunov", id="74", type="Start"
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
			RNG<double> rng = msg.JeOdchod ? _odchod
				: msg.PrisielSanitkou ? (RNG<double>)_prichodSanitka
				: _prichodSamostatne;
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
		public new AgentPresunov MyAgent
		{
			get
			{
				return (AgentPresunov)base.MyAgent;
			}
		}
	}
}
