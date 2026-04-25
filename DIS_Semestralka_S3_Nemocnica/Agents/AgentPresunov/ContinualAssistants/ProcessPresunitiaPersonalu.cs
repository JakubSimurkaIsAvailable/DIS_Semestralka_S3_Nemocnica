using OSPABA;
using Agents.AgentPresunov;
using Simulation;

namespace Agents.AgentPresunov.ContinualAssistants
{
	//meta! id="75"
	public class ProcessPresunitiaPersonalu : OSPABA.Process
	{
		public ProcessPresunitiaPersonalu(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		// TODO: doplnit spravne rozdelenie pre cas presunu personalu
		private OSPRNG.TriangularRNG _presunPersonalu = new OSPRNG.TriangularRNG(15, 20, 45);
		private HashSet<int> _active = new HashSet<int>();

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			_active.Clear();
		}

        //meta! sender="AgentPresunov", id="76", type="Start"
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
			double cas = msg.JePresunNaOsetrenie
				? Math.Max(_presunPersonalu.Sample(), _presunPersonalu.Sample())
				: _presunPersonalu.Sample();
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
