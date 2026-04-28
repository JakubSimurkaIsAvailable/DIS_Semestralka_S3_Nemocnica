using OSPABA;

namespace Simulation
{
	public class Mc : OSPABA.IdList
	{
		//meta! userInfo="Generated code: do not modify", tag="begin"
		public const int ZaradenieDoRaduOsetrenie = 1019;
		public const int ZdrojePrideleneVV = 1020;
		public const int ZdrojePrideleneOsetrenie = 1021;
		public const int OdchodPacienta = 1002;
		public const int PrichodPacienta = 1003;
		public const int VysetreniePacienta = 1005;
		public const int PresunPacienta = 1015;
		public const int PresunPersonalu = 1016;
		public const int VykonanieVstupnehoVysetrenia = 1008;
		public const int VykonanieOsetrenia = 1010;
		public const int UvolnenieAmbulancie = 1017;
		public const int ZaradenieDoRaduVV = 1018;
		//meta! tag="end"

		// 1..1000 range reserved for user
		public const int PrichodPacientaNaUrgent = 1;
		public const int UvolnenieZdrojovVstupneVysetrenie = 2;
		public const int UvolnenieZdrojovOsetrenie = 3;
		public const int OdchodPacientaZUrgentu = 4;
		public const int PresunPersonaluNaVstupneVysetrenie = 5;
		public const int PresunPersonaluNaOsetrenie = 6;
		public const int VstupneVysetrenieSkoncilo = 7;
		public const int OsetrenieSkoncilo = 8;
		public const int PresunutiePacientaSkoncilo = 9;
		public const int PresunutiePersonaluSkoncilo = 10;
	}
}