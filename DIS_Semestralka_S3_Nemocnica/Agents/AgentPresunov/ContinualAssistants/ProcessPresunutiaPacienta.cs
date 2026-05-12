using OSPABA;
using Agents.AgentPresunov;
using Simulation;

namespace Agents.AgentPresunov.ContinualAssistants
{
	//meta! id="73"
	public class ProcessPresunutiaPacienta : OSPABA.Process
	{
        public ProcessPresunutiaPacienta(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
		}

		//meta! sender="AgentPresunov", id="74", type="Start"
		public void ProcessStart(MessageForm message)
		{
			var msg = (MyMessage)message;
			double cas;
			var sim = (MySimulation)MySim;
			if (msg.JeOdchod)
			{
				cas = msg.CasOdchodu;
				sim.AnimPacientPohyb(msg.PacientId, cas, SimAnim.PesiVstup.X, SimAnim.PesiVstup.Y);
			}
			else
			{
				cas = msg.PrisielSanitkou
					? MyAgent.GenerujCasPrichoduPacientaSanitka()
					: MyAgent.GenerujCasPrichoduPacientaSamostatne();
				sim.AnimPacientPohyb(msg.PacientId, cas, SimAnim.VVRad.X, SimAnim.VVRad.Y);
			}
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
