using OSPABA;
using Simulation;
using Agents.AgentOsetrenia;

namespace Agents.AgentOsetrenia.ContinualAssistants
{
	//meta! id="40"
	public class ProcessOsetrenie : OSPABA.Process
	{
		public ProcessOsetrenie(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
		}

		//meta! sender="AgentOsetrenia", id="41", type="Start"
		public void ProcessStart(MessageForm message)
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
		override public void ProcessMessage(MessageForm message)
		{
			switch (message.Code)
			{
			case Mc.Start:
				ProcessStart(message);
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new AgentOsetrenia MyAgent
		{
			get
			{
				return (AgentOsetrenia)base.MyAgent;
			}
		}
	}
}
