using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIS_Semestralka_S3_Nemocnica.Generators
{
    /// <summary>
    /// Trieda je vytvorena pomocou AI, zdokumentovana v kapitole 1.1.1
    /// </summary>
    public class ExponencialnyGenerator
    {
        private Random _random;
        private double _lambda;

        /// <summary>
        /// Initializes a new instance of the ExponencialnyGenerator class.
        /// </summary>
        /// <param name="generatorGeneratorov">Master random generator for seeding</param>
        /// <param name="lambda">Rate parameter (lambda) of the exponential distribution. Must be positive.</param>
        public ExponencialnyGenerator(Random generatorGeneratorov, double lambda)
        {
            if (lambda <= 0)
            {
                throw new ArgumentException("Lambda (rate parameter) must be positive.", nameof(lambda));
            }

            _lambda = lambda;
            _random = new Random(generatorGeneratorov.Next());
        }

        /// <summary>
        /// Generates a random value from the exponential distribution using the inverse transform method.
        /// </summary>
        /// <returns>A random value following the exponential distribution with parameter lambda</returns>
        public double Generate()
        {
            double u = 1.0 - _random.NextDouble();
            return -Math.Log(u) / _lambda;
        }
    }
}
