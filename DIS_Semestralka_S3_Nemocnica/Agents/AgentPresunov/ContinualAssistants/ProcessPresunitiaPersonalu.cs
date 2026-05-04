using DIS_Semestralka_S3_Nemocnica.Generators;
using OSPABA;
using Agents.AgentPresunov;
using Simulation;

namespace Agents.AgentPresunov.ContinualAssistants
{
	//meta! id="75"
	public class ProcessPresunitiaPersonalu : OSPABA.Process
	{

		// TODO: doplnit spravne rozdelenie pre cas presunu personalu
		private TrojuholnikovyGenerator _presunPersonalu;
		public ProcessPresunitiaPersonalu(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
			_presunPersonalu = new TrojuholnikovyGenerator(((MySimulation)mySim).SeedRandom, 15, 20, 45);
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
				double tSestra  = sim.SestraJeUzVMiestnosti(msg.PacientId)  ? 0 : _presunPersonalu.Generate();
				double tLekar   = sim.LekarJeUzVMiestnosti(msg.PacientId)   ? 0 : _presunPersonalu.Generate();
				double tPacient = sim.PacientJeUzVMiestnosti(msg.PacientId) ? 0 : _presunPersonalu.Generate();
				cas = Math.Max(Math.Max(tSestra, tLekar), tPacient);
				sim.AnimStaffPohybDoMiestnosti(msg.PacientId, tSestra, tLekar);
				sim.AnimPacientPohybDoOsetrenia(msg.PacientId, tPacient);
			}
			else
			{
				cas = sim.SestraJeUzVMiestnosti(msg.PacientId) ? 0 : _presunPersonalu.Generate();
				sim.AnimSestryPohybDoMiestnosti(msg.PacientId, cas);
			}
			message.Code = Mc.PresunutiePersonaluSkoncilo;
			Hold(cas, message);
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
