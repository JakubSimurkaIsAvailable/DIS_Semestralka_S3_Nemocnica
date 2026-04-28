using OSPABA;
using Agents.AgentOsetrenia.ContinualAssistants;
using Simulation;

namespace Agents.AgentOsetrenia
{
	//meta! id="11"
	public class AgentOsetrenia : OSPABA.Agent
	{
		public AgentOsetrenia(int id, OSPABA.Simulation mySim, Agent parent) :
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
			new ManagerOsetrenia(SimId.ManagerOsetrenia, MySim, this);
			new ProcessOsetrenie(SimId.ProcessOsetrenie, MySim, this);
			//AddOwnMessage(Mc.UvolnenieAmbulancie);
			AddOwnMessage(Mc.VykonanieOsetrenia);
		}
		//meta! tag="end"
	}
}
