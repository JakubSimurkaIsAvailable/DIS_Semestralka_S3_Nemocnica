using OSPABA;
using Simulation.Resources;

namespace Simulation
{

	public class MyMessage : OSPABA.MessageForm
	{
		public int PacientId { get; set; }
		public bool PrisielSanitkou { get; set; }
		public int Priorita { get; set; }
		public bool JeOdchod { get; set; }
		public bool JePresunNaOsetrenie { get; set; }
		public double CasVstupuDoRadu { get; set; }

		public Sestra? PriradenaSestrа { get; set; }
		public Lekar? PriradenyLekar { get; set; }
		public Miestnost? PridelenaMiestnost { get; set; }

		public bool PouzilaMiestnostA => PridelenaMiestnost?.IsA ?? false;

        public MyMessage(OSPABA.Simulation mySim) :
			base(mySim)
		{
		}

		public MyMessage(MyMessage original) :
			base(original)
		{
			// copy() is called in superclass
		}

		override public MessageForm CreateCopy()
		{
			return new MyMessage(this);
		}

		override protected void Copy(MessageForm message)
		{
			base.Copy(message);
			MyMessage original = (MyMessage)message;
			PacientId = original.PacientId;
			PrisielSanitkou = original.PrisielSanitkou;
			Priorita = original.Priorita;
			JeOdchod = original.JeOdchod;
			JePresunNaOsetrenie = original.JePresunNaOsetrenie;
			CasVstupuDoRadu = original.CasVstupuDoRadu;
			PriradenaSestrа = original.PriradenaSestrа;
			PriradenyLekar = original.PriradenyLekar;
			PridelenaMiestnost = original.PridelenaMiestnost;
        }
	}
}
