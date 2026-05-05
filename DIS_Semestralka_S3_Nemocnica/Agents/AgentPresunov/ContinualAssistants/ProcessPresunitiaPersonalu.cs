using Agents.AgentPresunov;
using DIS_Semestralka_S3_Nemocnica.Generators;
using DIS_Semestralka_S3_Nemocnica.Generators.Components;
using OSPABA;
using ScottPlot.Colormaps;
using Simulation;
using Simulation.Resources;

namespace Agents.AgentPresunov.ContinualAssistants
{
	//meta! id="75"
	public class ProcessPresunitiaPersonalu : OSPABA.Process
	{

		// TODO: doplnit spravne rozdelenie pre cas presunu personalu
		private TrojuholnikovyGenerator _presunPersonalu;
		private RozdelenieSpojite _prichodSanitka;
        public ProcessPresunitiaPersonalu(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
			_presunPersonalu = new TrojuholnikovyGenerator(((MySimulation)mySim).SeedRandom, 15, 20, 45);
            _prichodSanitka = new RozdelenieSpojite(((MySimulation)mySim).SeedRandom, 90, 200);
        }

		override public void PrepareReplication()
		{
			base.PrepareReplication();
		}

        //meta! sender="AgentPresunov", id="76", type="Start"
        public void ProcessStart(MessageForm message)
		{
			var msg = (MyMessage)message;
			var sim = (MySimulation)MySim;
			double cas;
			if (msg.JePresunNaOsetrenie)
			{
				double tSestra  = CasPresunutia(msg.PriradenaSestrа!, msg.PridelenaMiestnost!, sim);
				double tLekar   = CasPresunutia(msg.PriradenyLekar!,  msg.PridelenaMiestnost!, sim);
				double tPacient = sim.PacientJeUzVMiestnosti(msg.PacientId, msg.PridelenaMiestnost!) ? 0 : _presunPersonalu.Generate();
				cas = Math.Max(Math.Max(tSestra, tLekar), tPacient);
				sim.AnimStaffPohybDoMiestnosti(msg.PacientId, tSestra, tLekar);
				sim.AnimPacientPohybDoOsetrenia(msg.PacientId, tPacient);
			}
			else
			{
				cas = CasPresunutia(msg.PriradenaSestrа!, msg.PridelenaMiestnost!, sim);
				sim.AnimSestryPohybDoMiestnosti(msg.PacientId, cas);
			}
			message.Code = Mc.PresunutiePersonaluSkoncilo;
			Hold(cas, message);
		}

		private double CasPresunutia(Sestra sestra, Miestnost miestnost, MySimulation sim)
		{
			if (sim.SestraJeUzVMiestnosti(sestra, miestnost)) return 0;
			return sim.SestraJeVAmbulancii(sestra) ? _presunPersonalu.Generate() : _prichodSanitka.Generate();
		}

		private double CasPresunutia(Lekar lekar, Miestnost miestnost, MySimulation sim)
		{
			if (sim.LekarJeUzVMiestnosti(lekar, miestnost)) return 0;
			return sim.LekarJeVAmbulancii(lekar) ? _presunPersonalu.Generate() : _prichodSanitka.Generate();
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
			}
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		override public void ProcessMessage(MessageForm message)
		{
			switch (message.Code)
			{
			case Mc.Start:
				ProcessStart(message);
			break;

			case Mc.PresunutiePersonaluSkoncilo:
				AssistantFinished(message);
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new AgentPresunov MyAgent
		{
			get
			{
				return (AgentPresunov)base.MyAgent;
			}
		}
	}
}
