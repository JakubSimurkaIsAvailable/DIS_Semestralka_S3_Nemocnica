namespace Simulation
{
    public class PacientInfo
    {
        public int Id { get; set; }
        public bool PrisielSanitkou { get; set; }
        public int Priorita { get; set; }
        public string Stav { get; set; } = "Príchod";
        public double CasPrichodu { get; set; }
        public bool PouzilaMiestnostA { get; set; }
    }
}
