using OSPABA;
using Simulation;

namespace Agents.AgentZdrojov
{
	//meta! id="12"
	public class ManagerZdrojov : OSPABA.Manager
	{
		public ManagerZdrojov(int id, OSPABA.Simulation mySim, Agent myAgent) :
			base(id, mySim, myAgent)
		{
			Init();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();

			if (PetriNet != null)
			{
				PetriNet.Clear();
			}
		}

		//meta! sender="AgentUrgentu", id="28", type="Request"
		public void ProcessPridelenieZdrojovVstupneVysetrenie(MessageForm message)
		{
			var msg = (MyMessage)message;
			int skupinaKey = msg.PrisielSanitkou ? 0 : 1;
			MyAgent.PendingVstupneVysetrenie.Enqueue(msg, (skupinaKey, msg.PacientId));
			MyAgent.RadVVIds.Add(msg.PacientId);
			SkusSpustitVstupneVysetrenie();
		}

		//meta! sender="AgentUrgentu", id="27", type="Request"
		public void ProcessPridelenieZdrojovOsetrenie(MessageForm message)
		{
			var msg = (MyMessage)message;
			MyAgent.PendingOsetrenie.Enqueue(msg, (msg.Priorita, msg.PacientId));
			MyAgent.RadOsetreniaItems.Add((msg.PacientId, msg.Priorita));
			SkusSpustitOsetrenie();
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
			case Mc.UvolnenieZdrojovVstupneVysetrenie:
				MyAgent.VolneSestry++;
				MyAgent.VolneMiestnostiB++;
				SkusSpustitOsetrenie();
				SkusSpustitVstupneVysetrenie();
				break;

			case Mc.UvolnenieZdrojovOsetrenie:
				var msg = (MyMessage)message;
				MyAgent.VolneLekari++;
				MyAgent.VolneSestry++;
				if (msg.PouzilaMiestnostA) MyAgent.VolneMiestnostiA++;
				else MyAgent.VolneMiestnostiB++;
				SkusSpustitOsetrenie();
				SkusSpustitVstupneVysetrenie();
				break;
			}
		}

		private void SkusSpustitVstupneVysetrenie()
		{
			if (MyAgent.PendingVstupneVysetrenie.Count == 0
				|| MyAgent.VolneSestry == 0
				|| MyAgent.VolneMiestnostiB == 0)
				return;

			MyAgent.VolneSestry--;
			MyAgent.VolneMiestnostiB--;
			var spusteny = MyAgent.PendingVstupneVysetrenie.Dequeue();
			MyAgent.RadVVIds.Remove(spusteny.PacientId);
			Response(spusteny);
		}

		private void SkusSpustitOsetrenie()
		{
			if (MyAgent.PendingOsetrenie.Count == 0
				|| MyAgent.VolneLekari == 0
				|| MyAgent.VolneSestry == 0)
				return;

			MyAgent.PendingOsetrenie.TryPeek(out var pacient, out _);

			bool pouzijA;
			if (pacient!.Priorita <= 2)
			{
				if (MyAgent.VolneMiestnostiA == 0) return;
				pouzijA = true;
			}
			else if (pacient.Priorita <= 4)
			{
				if (MyAgent.VolneMiestnostiB > 0) pouzijA = false;
				else if (MyAgent.VolneMiestnostiA > 0) pouzijA = true;
				else return;
			}
			else
			{
				if (MyAgent.VolneMiestnostiB == 0) return;
				pouzijA = false;
			}

			MyAgent.PendingOsetrenie.Dequeue();
			MyAgent.RadOsetreniaItems.RemoveAll(x => x.Id == pacient.PacientId);
			MyAgent.VolneLekari--;
			MyAgent.VolneSestry--;
			if (pouzijA) MyAgent.VolneMiestnostiA--;
			else MyAgent.VolneMiestnostiB--;

			pacient.PouzilaMiestnostA = pouzijA;
			Response(pacient);
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

			case Mc.PridelenieZdrojovOsetrenie:
				ProcessPridelenieZdrojovOsetrenie(message);
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new AgentZdrojov MyAgent
		{
			get
			{
				return (AgentZdrojov)base.MyAgent;
			}
		}
	}
}
