using OSPABA;
using Simulation;
using Agents.AgentPresunov.ContinualAssistants;
using DIS_Semestralka_S3_Nemocnica.Generators;
using DIS_Semestralka_S3_Nemocnica.Generators.Components;

namespace Agents.AgentPresunov
{
	//meta! id="68"
	public class AgentPresunov : OSPABA.Agent
	{
		private TrojuholnikovyGenerator _genPrichodPacientaSamostatne;
		private RozdelenieSpojite      _genPrichodPacientaSanitka;
		private RozdelenieSpojite      _genOdchodPacienta;
		private TrojuholnikovyGenerator _genPresunPersonalu;
		private RozdelenieSpojite      _genPresunPersonaluZCakarni;

		public double GenerujCasPrichoduPacientaSamostatne() => _genPrichodPacientaSamostatne.Generate();
		public double GenerujCasPrichoduPacientaSanitka()    => _genPrichodPacientaSanitka.Generate();
		public double GenerujCasOdchoduPacienta()            => _genOdchodPacienta.Generate();
		public double GenerujCasPresunuPersonalu()           => _genPresunPersonalu.Generate();
		public double GenerujCasPresunuPersonaluZCakarni()   => _genPresunPersonaluZCakarni.Generate();

		public AgentPresunov(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
            AddOwnMessage(Mc.PrichodPacientaNaUrgent);
            AddOwnMessage(Mc.OdchodPacientaZUrgentu);
            AddOwnMessage(Mc.PresunPersonaluNaVstupneVysetrenie);
            AddOwnMessage(Mc.PresunPersonaluNaOsetrenie);
            AddOwnMessage(Mc.PresunutiePacientaSkoncilo);
            AddOwnMessage(Mc.PresunutiePersonaluSkoncilo);
        }

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			var seed = ((MySimulation)MySim).SeedRandom;
			new ManagerPresunov(SimId.ManagerPresunov, MySim, this);
			_genPrichodPacientaSamostatne = new TrojuholnikovyGenerator(seed, 120, 150, 300);
			_genPrichodPacientaSanitka    = new RozdelenieSpojite(seed, 90, 200);
			_genOdchodPacienta            = new RozdelenieSpojite(seed, 150, 240);
			new ProcessPresunutiaPacienta(SimId.ProcessPresunutiaPacienta, MySim, this);
			_genPresunPersonalu           = new TrojuholnikovyGenerator(seed, 15, 20, 45);
			_genPresunPersonaluZCakarni   = new RozdelenieSpojite(seed, 90, 200);
			new ProcessPresunitiaPersonalu(SimId.ProcessPresunitiaPersonalu, MySim, this);
			AddOwnMessage(Mc.PresunPacienta);
			AddOwnMessage(Mc.PresunPersonalu);
		}
		//meta! tag="end"
	}
}
