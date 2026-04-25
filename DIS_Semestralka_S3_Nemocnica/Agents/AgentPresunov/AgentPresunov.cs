using OSPABA;
using Simulation;
using Agents.AgentPresunov.ContinualAssistants;

namespace Agents.AgentPresunov
{
	//meta! id="68"
	public class AgentPresunov : OSPABA.Agent
	{
		public AgentPresunov(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new ManagerPresunov(SimId.ManagerPresunov, MySim, this);
			new ProcessPresunutiaPacienta(SimId.ProcessPresunutiaPacienta, MySim, this);
			new ProcessPresunitiaPersonalu(SimId.ProcessPresunitiaPersonalu, MySim, this);
			AddOwnMessage(Mc.PresunPacienta);
			AddOwnMessage(Mc.PresunPersonalu);
			AddOwnMessage(Mc.PrichodPacientaNaUrgent);
		}
		//meta! tag="end"
	}
}
