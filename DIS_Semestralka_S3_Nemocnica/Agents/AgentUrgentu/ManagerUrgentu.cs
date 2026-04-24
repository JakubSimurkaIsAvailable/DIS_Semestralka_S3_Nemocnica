using OSPABA;
using Simulation;

namespace Agents.AgentUrgentu
{
	//meta! id="3"
	public class ManagerUrgentu : OSPABA.Manager
	{
		public ManagerUrgentu(int id, OSPABA.Simulation mySim, Agent myAgent) :
			base(id, mySim, myAgent)
		{
			Init();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication

			if (PetriNet != null)
			{
				PetriNet.Clear();
			}
		}

		//meta! sender="AgentModelu", id="8", type="Request"
		public void ProcessVysetreniePacienta(MessageForm message)
		{
		}

		//meta! sender="AgentVstupnehoVysetrenia", id="22", type="Response"
		public void ProcessVykonanieVstupnehoVysetrenia(MessageForm message)
		{
		}

		//meta! sender="AgentPresunov", id="77", type="Response"
		public void ProcessPresunPacienta(MessageForm message)
		{
		}

		//meta! sender="AgentZdrojov", id="28", type="Response"
		public void ProcessPridelenieZdrojovVstupneVysetrenie(MessageForm message)
		{
		}

		//meta! sender="AgentOsetrenia", id="24", type="Response"
		public void ProcessVykonanieOsetrenia(MessageForm message)
		{
		}

		//meta! sender="AgentPresunov", id="78", type="Response"
		public void ProcessPresunPersonalu(MessageForm message)
		{
		}

		//meta! sender="AgentZdrojov", id="27", type="Response"
		public void ProcessPridelenieZdrojovOsetrenie(MessageForm message)
		{
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
			}
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		public void Init()
		{
		}

		override public void ProcessMessage(MessageForm message)
		{
			switch (message.Code)
			{
			case Mc.PridelenieZdrojovVstupneVysetrenie:
				ProcessPridelenieZdrojovVstupneVysetrenie(message);
			break;

			case Mc.PresunPersonalu:
				ProcessPresunPersonalu(message);
			break;

			case Mc.VysetreniePacienta:
				ProcessVysetreniePacienta(message);
			break;

			case Mc.VykonanieVstupnehoVysetrenia:
				ProcessVykonanieVstupnehoVysetrenia(message);
			break;

			case Mc.PridelenieZdrojovOsetrenie:
				ProcessPridelenieZdrojovOsetrenie(message);
			break;

			case Mc.VykonanieOsetrenia:
				ProcessVykonanieOsetrenia(message);
			break;

			case Mc.PresunPacienta:
				ProcessPresunPacienta(message);
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new AgentUrgentu MyAgent
		{
			get
			{
				return (AgentUrgentu)base.MyAgent;
			}
		}
	}
}
