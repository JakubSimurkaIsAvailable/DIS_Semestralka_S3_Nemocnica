using Agents.AgentOkolia;
using OSPABA;
using Agents.AgentModelu;
using Agents.AgentUrgentu;
using Agents.AgentPresunov;
using Agents.AgentVstupnehoVysetrenia;
using Agents.AgentZdrojov;
using Agents.AgentOsetrenia;

namespace Simulation
{
	public class MySimulation : OSPABA.Simulation
	{
		public MySimulation()
		{
			Init();
		}

		override public void PrepareSimulation()
		{
			base.PrepareSimulation();
			// Create global statistcis
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Reset entities, queues, local statistics, etc...
		}

		override public void ReplicationFinished()
		{
			// Collect local statistics into global, update UI, etc...
			base.ReplicationFinished();
		}

		override public void SimulationFinished()
		{
			// Display simulation results
			base.SimulationFinished();
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
		public AgentModelu AgentModelu
		{ get; set; }
		public AgentOkolia AgentOkolia
		{ get; set; }
		public AgentUrgentu AgentUrgentu
		{ get; set; }
		public AgentPresunov AgentPresunov
		{ get; set; }
		public AgentVstupnehoVysetrenia AgentVstupnehoVysetrenia
		{ get; set; }
		public AgentOsetrenia AgentOsetrenia
		{ get; set; }
		public AgentZdrojov AgentZdrojov
		{ get; set; }
		//meta! tag="end"
	}
}