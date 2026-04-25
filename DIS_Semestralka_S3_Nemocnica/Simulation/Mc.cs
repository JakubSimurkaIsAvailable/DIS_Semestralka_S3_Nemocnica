using OSPABA;

namespace Simulation
{
	public class Mc : OSPABA.IdList
	{
		//meta! userInfo="Generated code: do not modify", tag="begin"
		public const int OdchodPacienta = 1002;
		public const int PrichodPacienta = 1003;
		public const int VysetreniePacienta = 1005;
		public const int PresunPacienta = 1015;
		public const int PresunPersonalu = 1016;
		public const int VykonanieVstupnehoVysetrenia = 1008;
		public const int UvolnenieAmbulancie = 1009;
		public const int VykonanieOsetrenia = 1010;
		public const int PridelenieZdrojovOsetrenie = 1013;
		public const int PridelenieZdrojovVstupneVysetrenie = 1014;
		//meta! tag="end"

		// 1..1000 range reserved for user
		public const int PrichodPacientaNaUrgent = 1;
		public const int UvolnenieZdrojovVstupneVysetrenie = 2;
		public const int UvolnenieZdrojovOsetrenie = 3;
	}
}
