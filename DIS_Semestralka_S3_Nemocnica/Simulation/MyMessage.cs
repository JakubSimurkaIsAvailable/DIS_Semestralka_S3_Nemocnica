using OSPABA;

namespace Simulation
{

	public class MyMessage : OSPABA.MessageForm
	{
		public int PacientId { get; set; }
		public bool PrisielSanitkou { get; set; }
		public int Priorita { get; set; }
		public bool PouzilaMiestnostA { get; set; }
		public bool JeOdchod { get; set; }
		public bool JePresunNaOsetrenie { get; set; }
		public double CasVstupuDoRadu { get; set; }

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
			PouzilaMiestnostA = original.PouzilaMiestnostA;
			JeOdchod = original.JeOdchod;
			JePresunNaOsetrenie = original.JePresunNaOsetrenie;
			CasVstupuDoRadu = original.CasVstupuDoRadu;
        }
	}
}