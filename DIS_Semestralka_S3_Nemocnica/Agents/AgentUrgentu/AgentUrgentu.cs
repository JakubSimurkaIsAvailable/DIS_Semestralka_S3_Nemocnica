using OSPABA;
using Simulation;

namespace Agents.AgentUrgentu
{
	//meta! id="3"
	public class AgentUrgentu : OSPABA.Agent
	{
		public AgentUrgentu(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new ManagerUrgentu(SimId.ManagerUrgentu, MySim, this);
            AddOwnMessage(Mc.PresunPacienta);
            AddOwnMessage(Mc.VysetreniePacienta);
            AddOwnMessage(Mc.VykonanieVstupnehoVysetrenia);
            AddOwnMessage(Mc.ZdrojePrideleneOsetrenie);
            AddOwnMessage(Mc.ZdrojePrideleneVV);
            AddOwnMessage(Mc.VykonanieOsetrenia);
            AddOwnMessage(Mc.PresunPersonalu);
        }
		//meta! tag="end"
	}
}
