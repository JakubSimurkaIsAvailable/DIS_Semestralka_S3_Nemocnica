using Agents.AgentModelu;
using Agents.AgentOkolia;
using Agents.AgentOsetrenia;
using Agents.AgentPresunov;
using Agents.AgentUrgentu;
using Agents.AgentVstupnehoVysetrenia;
using Agents.AgentZdrojov;
using System.Collections.Concurrent;

namespace Simulation
{
	public class MySimulation : OSPABA.Simulation
	{
		public Random SeedRandom { get; private set; } = new Random();

		public ConcurrentDictionary<int, PacientInfo> Pacienti { get; } = new();
		public volatile bool Zastavit;

		public void NastavSeed(int seed) => SeedRandom = new Random(seed);
		public void NastavNahodny() => SeedRandom = new Random();

		public void AktualizujStavPacienta(int id, string stav)
		{
			if (Pacienti.TryGetValue(id, out var info))
				info.Stav = stav;
		}

		public void AktualizujPriorituPacienta(int id, int priorita)
		{
			if (Pacienti.TryGetValue(id, out var info))
				info.Priorita = priorita;
		}

		public MySimulation()
		{
			Init();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			if (Zastavit)
			{
				StopSimulation();
				return;
			}
			Pacienti.Clear();
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			AgentModelu = new AgentModelu(SimId.AgentModelu, this, null);
			AgentOkolia = new AgentOkolia(SimId.AgentOkolia, this, AgentModelu);
			AgentUrgentu = new AgentUrgentu(SimId.AgentUrgentu, this, AgentModelu);
			AgentPresunov = new AgentPresunov(SimId.AgentPresunov, this, AgentUrgentu);
			AgentVstupnehoVysetrenia = new AgentVstupnehoVysetrenia(SimId.AgentVstupnehoVysetrenia, this, AgentUrgentu);
			AgentOsetrenia = new AgentOsetrenia(SimId.AgentOsetrenia, this, AgentUrgentu);
			AgentZdrojov = new AgentZdrojov(SimId.AgentZdrojov, this, AgentUrgentu);
		}
		public AgentModelu AgentModelu { get; set; } = null!;
		public AgentOkolia AgentOkolia { get; set; } = null!;
		public AgentUrgentu AgentUrgentu { get; set; } = null!;
		public AgentPresunov AgentPresunov { get; set; } = null!;
		public AgentVstupnehoVysetrenia AgentVstupnehoVysetrenia { get; set; } = null!;
		public AgentOsetrenia AgentOsetrenia { get; set; } = null!;
		public AgentZdrojov AgentZdrojov { get; set; } = null!;
		//meta! tag="end"
	}
}
