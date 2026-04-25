using OSPABA;
using Simulation;
using Agents.AgentOkolia.ContinualAssistants;

namespace Agents.AgentOkolia
{
	//meta! id="2"
	public class AgentOkolia : OSPABA.Agent
	{
		public AgentOkolia(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
		}

		public int PocetPacientov { get; set; }

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			PocetPacientov = 0;
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new ManagerOkolia(SimId.ManagerOkolia, MySim, this);
			new PrichodPacientaSanitka(SimId.PrichodPacientaSanitka, MySim, this);
			new PrichodPacienta(SimId.PrichodPacienta, MySim, this);
			AddOwnMessage(Mc.OdchodPacienta);
		}
		//meta! tag="end"

		public void ZacniPlanovaniePacientov()
		{
			MyMessage sprava = new MyMessage(MySim);
			sprava.Addressee = FindAssistant(SimId.PrichodPacienta);
			MyManager.StartContinualAssistant(sprava);

			sprava = new MyMessage(MySim);
			sprava.Addressee = FindAssistant(SimId.PrichodPacientaSanitka);
			MyManager.StartContinualAssistant(sprava);
        }
	}
}