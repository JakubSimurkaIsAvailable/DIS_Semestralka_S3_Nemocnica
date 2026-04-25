using Simulation;

Console.WriteLine("Spustam simulaciu...");
try
{
    var sim = new MySimulation();
    Console.WriteLine("Simulacia vytvorena, spustam replikaciu...");
    sim.Simulate(1, 8 * 3600.0);
    Console.WriteLine("Simulacia dokoncena.");
}
catch (Exception ex)
{
    Console.WriteLine($"CHYBA: {ex}");
}
