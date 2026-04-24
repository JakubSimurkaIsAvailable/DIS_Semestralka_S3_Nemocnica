using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIS_Semestralka_S3_Nemocnica.Generators.Components
{
    public class RozdelenieDiskretne
    {
        int Min { get; set; }
        int Max { get; set; }
        Random random;
        public RozdelenieDiskretne(Random generatorGeneratorov, int min, int max)
        {
            Min = min;
            Max = max;
            random = new Random(generatorGeneratorov.Next());
        }
        public int Generate()
        {
            return random.Next(Min, Max);
        }
    }
}
