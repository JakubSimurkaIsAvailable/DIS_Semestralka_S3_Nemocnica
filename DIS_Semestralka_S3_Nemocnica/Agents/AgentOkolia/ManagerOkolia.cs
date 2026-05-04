using OSPABA;
using Simulation;

namespace Agents.AgentOkolia
{
	//meta! id="2"
	public class ManagerOkolia : OSPABA.Manager
	{
		public ManagerOkolia(int id, OSPABA.Simulation mySim, Agent myAgent) :
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
			MyAgent.ZacniPlanovaniePacientov();
        }

		//meta! sender="PrichodPacientaSanitka", id="57", type="Finish"
		public void ProcessFinishPrichodPacientaSanitka(MessageForm message)
		{
			var msg = (MyMessage)message;
			Console.WriteLine($"[{Cas()}] Pacient #{msg.PacientId} prisiel SANITKOU");
			((MySimulation)MySim).Pacienti[msg.PacientId] = new PacientInfo
			{
				Id = msg.PacientId,
				PrisielSanitkou = true,
				CasPrichodu = MySim.CurrentTime,
				Stav = "Presun"
			};
			((MySimulation)MySim).AnimPacientPrisiel(msg.PacientId, true);
			message.Code = Mc.PrichodPacienta;
			message.Addressee = MySim.FindAgent(SimId.AgentModelu);
			Notice(message);
        }

		//meta! sender="PrichodPacienta", id="55", type="Finish"
		public void ProcessFinishPrichodPacienta(MessageForm message)
		{
			var msg = (MyMessage)message;
			Console.WriteLine($"[{Cas()}] Pacient #{msg.PacientId} prisiel samostatne");
			((MySimulation)MySim).Pacienti[msg.PacientId] = new PacientInfo
			{
				Id = msg.PacientId,
				PrisielSanitkou = false,
				CasPrichodu = MySim.CurrentTime,
				Stav = "Presun"
			};
			((MySimulation)MySim).AnimPacientPrisiel(msg.PacientId, false);
			message.Code = Mc.PrichodPacienta;
			message.Addressee = MySim.FindAgent(SimId.AgentModelu);
            Notice(message);
		}

		private string Cas() =>
			TimeSpan.FromSeconds(MySim.CurrentTime).ToString(@"hh\:mm\:ss");

		//meta! sender="AgentModelu", id="5", type="Notice"
		public void ProcessOdchodPacienta(MessageForm message)
		{
			var msg = (MyMessage)message;
			var sim = (MySimulation)MySim;
			sim.AnimPacientOdsiel(msg.PacientId);
			if (sim.Pacienti.TryRemove(msg.PacientId, out var info))
			{
				double dobaVSysteme = MySim.CurrentTime - info.CasPrichodu;
				sim.LocDobaVSysteme.AddValue(dobaVSysteme);
				sim.LocPocetPacienti++;
				if (info.PrisielSanitkou)
				{
					sim.LocDobaVSystemeSanitka.AddValue(dobaVSysteme);
					sim.LocPocetSanitka++;
				}
				else
				{
					sim.LocDobaVSystemePeso.AddValue(dobaVSysteme);
					sim.LocPocetPeso++;
				}
			}
			sim.PocetVybavenych++;
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
			case Mc.OdchodPacienta:
				ProcessOdchodPacienta(message);
			break;

			case Mc.Finish:
				switch (message.Sender.Id)
				{
				case SimId.PrichodPacientaSanitka:
					ProcessFinishPrichodPacientaSanitka(message);
				break;

				case SimId.PrichodPacienta:
					ProcessFinishPrichodPacienta(message);
				break;
				}
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new AgentOkolia MyAgent
		{
			get
			{
				return (AgentOkolia)base.MyAgent;
			}
		}
	}
}