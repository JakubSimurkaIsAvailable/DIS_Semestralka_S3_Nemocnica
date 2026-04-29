using OSPABA;
using Simulation;

namespace Agents.AgentModelu
{
	//meta! id="1"
	public class AgentModelu : OSPABA.Agent
	{
		public AgentModelu(int id, OSPABA.Simulation mySim, Agent parent) :
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
			new ManagerModelu(SimId.ManagerModelu, MySim, this);
			AddOwnMessage(Mc.VysetreniePacienta);
			AddOwnMessage(Mc.PrichodPacienta);
		}
		//meta! tag="end"
	}
}
