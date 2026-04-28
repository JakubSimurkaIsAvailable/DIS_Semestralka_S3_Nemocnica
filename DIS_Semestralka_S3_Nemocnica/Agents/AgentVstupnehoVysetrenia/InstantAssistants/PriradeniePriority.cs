using OSPABA;
using Simulation;
using Agents.AgentVstupnehoVysetrenia;
using DIS_Semestralka_S3_Nemocnica.Generators;

namespace Agents.AgentVstupnehoVysetrenia.InstantAssistants
{
	//meta! id="66"
	public class PriradeniePriority : OSPABA.Action
	{
        // Priorita pre pacienta "Prišiel sám"
		private readonly PercentTable _prioritySamPT;


        // Priorita pre pacienta "Privezený sanitkou"
		private readonly PercentTable _priorityAmbPT;
        public PriradeniePriority(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
            var seed = ((MySimulation)mySim).SeedRandom;
			double[] pravSamPT = { 0.10, 0.20, 0.15, 0.25, 0.30 };
			double[] priority = { 1, 2, 3, 4, 5 };
			double[] pravAmbPT = { 0.30, 0.25, 0.20, 0.15, 0.10 };
            _prioritySamPT = new PercentTable(seed, pravSamPT, priority);
			_priorityAmbPT = new PercentTable(seed, pravAmbPT, priority);
        }

		override public void Execute(MessageForm message)
		{
			int priorita;
			if (((MyMessage)message).PrisielSanitkou)
			{
				priorita = (int)_priorityAmbPT.Generate();
			}
			else
			{
				priorita = (int)_prioritySamPT.Generate();
			}
            ((MyMessage)message).Priorita = priorita;
		}
		public new AgentVstupnehoVysetrenia MyAgent
		{
			get
			{
				return (AgentVstupnehoVysetrenia)base.MyAgent;
			}
		}
	}
}
