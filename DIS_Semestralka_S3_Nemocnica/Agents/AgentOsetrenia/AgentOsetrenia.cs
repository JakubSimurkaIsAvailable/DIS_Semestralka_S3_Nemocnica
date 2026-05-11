using OSPABA;
using Agents.AgentOsetrenia.ContinualAssistants;
using Simulation;
using DIS_Semestralka_S3_Nemocnica.Generators;
using DIS_Semestralka_S3_Nemocnica.Generators.Components;

namespace Agents.AgentOsetrenia
{
	//meta! id="11"
	public class AgentOsetrenia : OSPABA.Agent
	{
		private SpojityEmpirickyGenerator _genOsePeso;
		private RozdelenieSpojite _genOseSanitka;

		public double GenerujTrvanieOsetrenia(bool prisielSanitkou) =>
			(prisielSanitkou ? _genOseSanitka.Generate() : _genOsePeso.Generate()) * 60;

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
			var seed = ((MySimulation)MySim).SeedRandom;
			new ManagerOsetrenia(SimId.ManagerOsetrenia, MySim, this);
			_genOsePeso    = new SpojityEmpirickyGenerator(seed, new List<double> { 10, 12, 14 }, new List<double> { 12, 14, 18 }, new List<double> { 0.1, 0.6, 0.3 });
			_genOseSanitka = new RozdelenieSpojite(seed, 15, 30);
			new ProcessOsetrenie(SimId.ProcessOsetrenie, MySim, this);
			//AddOwnMessage(Mc.UvolnenieAmbulancie);
			AddOwnMessage(Mc.VykonanieOsetrenia);
			AddOwnMessage(Mc.OsetrenieSkoncilo);
		}
		//meta! tag="end"
	}
}
