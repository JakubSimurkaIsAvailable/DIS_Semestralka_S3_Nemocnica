namespace Simulation.Resources
{
    public abstract class Miestnost
    {
        public int Id { get; }
        public abstract bool IsA { get; }

        protected Miestnost(int id) => Id = id;
    }

    public class MiestnostA : Miestnost
    {
        public override bool IsA => true;
        public MiestnostA(int id) : base(id) { }
    }

    public class MiestnostB : Miestnost
    {
        public override bool IsA => false;
        public MiestnostB(int id) : base(id) { }
    }
}
