using OSPABA;
using Simulation;
using Agents.AgentVstupnehoVysetrenia;

namespace Agents.AgentVstupnehoVysetrenia.InstantAssistants
{
	//meta! id="66"
	public class PriradeniePriority : OSPABA.Action
	{
        // Priorita pre pacienta "Prišiel sám"
        private static readonly OSPRNG.EmpiricPair<int>[] _prioritySam = {
			new OSPRNG.EmpiricPair<int>(new OSPRNG.UniformDiscreteRNG(1, 1), 0.10),
			new OSPRNG.EmpiricPair<int>(new OSPRNG.UniformDiscreteRNG(2, 2), 0.20),
			new OSPRNG.EmpiricPair<int>(new OSPRNG.UniformDiscreteRNG(3, 3), 0.15),
			new OSPRNG.EmpiricPair<int>(new OSPRNG.UniformDiscreteRNG(4, 4), 0.25),
			new OSPRNG.EmpiricPair<int>(new OSPRNG.UniformDiscreteRNG(5, 5), 0.30),
		};
        private OSPRNG.EmpiricRNG<int> _prioritySamRNG = new OSPRNG.EmpiricRNG<int>(_prioritySam);

        // Priorita pre pacienta "Privezený sanitkou"
        private static readonly OSPRNG.EmpiricPair<int>[] _priorityAmb = {
			new OSPRNG.EmpiricPair<int>(new OSPRNG.UniformDiscreteRNG(1, 1), 0.30),
			new OSPRNG.EmpiricPair<int>(new OSPRNG.UniformDiscreteRNG(2, 2), 0.25),
			new OSPRNG.EmpiricPair<int>(new OSPRNG.UniformDiscreteRNG(3, 3), 0.20),
			new OSPRNG.EmpiricPair<int>(new OSPRNG.UniformDiscreteRNG(4, 4), 0.15),
			new OSPRNG.EmpiricPair<int>(new OSPRNG.UniformDiscreteRNG(5, 5), 0.10),
		};
        private OSPRNG.EmpiricRNG<int> _priorityAmbRNG = new OSPRNG.EmpiricRNG<int>(_priorityAmb);
        public PriradeniePriority(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		override public void Execute(MessageForm message)
		{
			int priorita;
			if (((MyMessage)message).PrisielSanitkou)
			{
				priorita = _priorityAmbRNG.Sample();
			}
			else
			{
				priorita = _prioritySamRNG.Sample();
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
