using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIS_Semestralka_S3_Nemocnica.Generators
{
    public class PercentTable
    {
        Random random;
        double[] chances;
        double[] values;
        public PercentTable(Random generatorGeneratorov, double[] chances, double[] values)
        {
            random = new Random(generatorGeneratorov.Next());
            this.chances = chances;
            this.values = values;
        }

        public double Generate()
        {
            double randomValue = random.NextDouble();
            double cumulativeChance = 0.0;
            for (int i = 0; i < chances.Length; i++)
            {
                cumulativeChance += chances[i];
                if (randomValue < cumulativeChance)
                {
                    return values[i];
                }
            }
            return values[values.Length - 1];
        }
    }
}
