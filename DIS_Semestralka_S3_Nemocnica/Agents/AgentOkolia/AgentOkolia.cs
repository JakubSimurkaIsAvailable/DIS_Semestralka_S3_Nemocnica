using OSPABA;
using Simulation;
using Agents.AgentOkolia.ContinualAssistants;
using DIS_Semestralka_S3_Nemocnica.Collectors;

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

		// ── Lokálne štatistiky (reset na začiatku každej replikácie) ──
		public volatile int LocPocetPacienti;
		public volatile int LocPocetPeso;
		public volatile int LocPocetSanitka;
		public StatisticsCollector LocDobaVSysteme        { get; private set; } = new();
		public StatisticsCollector LocDobaVSystemePeso    { get; private set; } = new();
		public StatisticsCollector LocDobaVSystemeSanitka { get; private set; } = new();

		public void ResetLocalStats()
		{
			LocPocetPacienti = 0; LocPocetPeso = 0; LocPocetSanitka = 0;
			LocDobaVSysteme        = new StatisticsCollector();
			LocDobaVSystemePeso    = new StatisticsCollector();
			LocDobaVSystemeSanitka = new StatisticsCollector();
		}

		override public void PrepareReplication()
		{
			ResetLocalStats();
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