using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIS_Semestralka_S3_Nemocnica.Generators.Components
{
    public class RozdelenieSpojite
    {
        double Min { get; set; }
        double Max { get; set; }
        Random random;
        public RozdelenieSpojite(Random generatorGeneratorov, double min, double max)
        {
            Min = min;
            Max = max;
            random = new Random(generatorGeneratorov.Next());
        }
        public double Generate()
        {
            return random.NextDouble() * (Max - Min) + Min;
        }
    }
}
