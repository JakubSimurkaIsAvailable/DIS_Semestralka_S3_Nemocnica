using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIS_Semestralka_S3_Nemocnica.Generators
{
    public class TrojuholnikovyGenerator
    {
        private readonly Random _random;
        private readonly double _minimum;
        private readonly double _maximum;
        private readonly double _modus;

        public TrojuholnikovyGenerator(Random generatorGeneratorov, double minimum, double modus, double maximum)
        {
            if (minimum > modus || modus > maximum)
            {
                throw new ArgumentException("Parameters must satisfy a <= c <= b.");
            }

            if (minimum == maximum)
            {
                throw new ArgumentException("Parameters a and b cannot be equal.");
            }

            _minimum = minimum;
            _maximum = maximum;
            _modus = modus;
            _random = new Random(generatorGeneratorov.Next());
        }

        public double Generate()
        {
            double u = _random.NextDouble();
            double fc = (_modus - _minimum) / (_maximum - _minimum);

            if (u < fc)
            {
                return _minimum + Math.Sqrt(u * (_maximum - _minimum) * (_modus - _minimum));
            }
            else
            {
                return _maximum - Math.Sqrt((1.0 - u) * (_maximum - _minimum) * (_maximum - _modus));
            }
        }
    }
}
