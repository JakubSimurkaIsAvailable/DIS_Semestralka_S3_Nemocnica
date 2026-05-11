using OSPABA;
using Agents.AgentVstupnehoVysetrenia.ContinualAssistants;
using Simulation;
using Agents.AgentVstupnehoVysetrenia.InstantAssistants;
using DIS_Semestralka_S3_Nemocnica.Generators;
using DIS_Semestralka_S3_Nemocnica.Generators.Components;

namespace Agents.AgentVstupnehoVysetrenia
{
	//meta! id="10"
	public class AgentVstupnehoVysetrenia : OSPABA.Agent
	{
		private PercentTable _genPrioritaPeso;
		private PercentTable _genPrioritaSanitka;
		private SpojityEmpirickyGenerator _genVVPeso;
		private RozdelenieDiskretne _genVVSanitka;

		public int GenerujPrioritu(bool prisielSanitkou) =>
			(int)(prisielSanitkou ? _genPrioritaSanitka.Generate() : _genPrioritaPeso.Generate());

		public double GenerujTrvanieVV(bool prisielSanitkou) =>
			prisielSanitkou ? _genVVSanitka.Generate() * 60 : _genVVPeso.Generate() * 60;

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
			var seed = ((MySimulation)MySim).SeedRandom;
			new ManagerVstupnehoVysetrenia(SimId.ManagerVstupnehoVysetrenia, MySim, this);
			_genPrioritaPeso    = new PercentTable(seed, [0.10, 0.20, 0.15, 0.25, 0.30], [1, 2, 3, 4, 5]);
			_genPrioritaSanitka = new PercentTable(seed, [0.30, 0.25, 0.20, 0.15, 0.10], [1, 2, 3, 4, 5]);
			new PriradeniePriority(SimId.PriradeniePriority, MySim, this);
			_genVVPeso    = new SpojityEmpirickyGenerator(seed, new List<double> { 3, 5 }, new List<double> { 5, 9 }, new List<double> { 0.6, 0.4 });
			_genVVSanitka = new RozdelenieDiskretne(seed, 4, 9);
			new ProcessVstupneVysetrenie(SimId.ProcessVstupneVysetrenie, MySim, this);
			AddOwnMessage(Mc.VykonanieVstupnehoVysetrenia);
			AddOwnMessage(Mc.VstupneVysetrenieSkoncilo);
			//AddOwnMessage(Mc.UvolnenieAmbulancie);
		}
		//meta! tag="end"
	}
}
