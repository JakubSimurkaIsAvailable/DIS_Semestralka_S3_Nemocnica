using OSPABA;
using Agents.AgentVstupnehoVysetrenia.ContinualAssistants;
using Simulation;
using Agents.AgentVstupnehoVysetrenia.InstantAssistants;

namespace Agents.AgentVstupnehoVysetrenia
{
	//meta! id="10"
	public class AgentVstupnehoVysetrenia : OSPABA.Agent
	{
		public AgentVstupnehoVysetrenia(int id, OSPABA.Simulation mySim, Agent parent) :
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
			new ManagerVstupnehoVysetrenia(SimId.ManagerVstupnehoVysetrenia, MySim, this);
			new ProcessVstupneVysetrenie(SimId.ProcessVstupneVysetrenie, MySim, this);
			new PriradeniePriority(SimId.PriradeniePriority, MySim, this);
			AddOwnMessage(Mc.VykonanieVstupnehoVysetrenia);
		}
		//meta! tag="end"
	}
}
