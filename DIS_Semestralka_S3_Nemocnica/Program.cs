using DIS_Semestralka_S3_Nemocnica.Generators;
using System.Windows.Forms;
/*
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.Run(new DIS_Semestralka_S3_Nemocnica.Form1());
*/



GammaGenerator gammaGenerator = new GammaGenerator(new Random(), 4.37, 67.5);

var values = Enumerable.Range(0, 1477).Select(_ => 56 + gammaGenerator.Generate());
File.WriteAllLines("gamma_test.txt", values.Select(v => v.ToString()));
Console.WriteLine("Hotovo: gamma_test.txt");
