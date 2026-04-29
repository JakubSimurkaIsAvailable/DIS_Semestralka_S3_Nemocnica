using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIS_Semestralka_S3_Nemocnica.Generators
{
    /// <summary>
    /// Generátor náhodných čísel z Gamma rozdelenia pomocou Marsaglia-Tsang metódy.
    /// Trieda vytvorená pomocou AI, kapitola 1.1.1
    /// </summary>
    public class GammaGenerator
    {
        private Random _random;
        private double _alpha; // shape parameter
        private double _beta;  // scale parameter (1/lambda)

        /// <summary>
        /// Initializes a new instance of the GammaGenerator class.
        /// </summary>
        /// <param name="generatorGeneratorov">Master random generator for seeding</param>
        /// <param name="alpha">Shape parameter (alpha). Must be positive.</param>
        /// <param name="beta">Scale parameter (beta = 1/lambda). Must be positive.</param>
        public GammaGenerator(Random generatorGeneratorov, double alpha, double beta)
        {
            if (alpha <= 0)
                throw new ArgumentException("Alpha (shape parameter) must be positive.", nameof(alpha));
            if (beta <= 0)
                throw new ArgumentException("Beta (scale parameter) must be positive.", nameof(beta));

            _alpha = alpha;
            _beta = beta;
            _random = new Random(generatorGeneratorov.Next());
        }

        /// <summary>
        /// Generates a random value from the Gamma distribution using the Marsaglia-Tsang method.
        /// </summary>
        /// <returns>A random value following the Gamma distribution with parameters alpha and beta.</returns>
        public double Generate()
        {
            // Pre alpha < 1 pouzijeme trik: Gamma(alpha) = Gamma(alpha+1) * U^(1/alpha)
            if (_alpha < 1.0)
            {
                double u = 1.0 - _random.NextDouble();
                return GenerateForAlphaAbove1(_alpha + 1.0) * Math.Pow(u, 1.0 / _alpha);
            }

            return GenerateForAlphaAbove1(_alpha);
        }

        /// <summary>
        /// Marsaglia-Tsang metóda pre alpha >= 1.
        /// </summary>
        private double GenerateForAlphaAbove1(double alpha)
        {
            double d = alpha - 1.0 / 3.0;
            double c = 1.0 / Math.Sqrt(9.0 * d);

            while (true)
            {
                double x, v;

                // Generuj x z normalneho rozdelenia (Box-Muller)
                do
                {
                    x = GenerateStandardNormal();
                    v = 1.0 + c * x;
                } while (v <= 0.0);

                v = v * v * v; // v = (1 + c*x)^3
                double u = 1.0 - _random.NextDouble();

                // Rychly accept test
                if (u < 1.0 - 0.0331 * (x * x) * (x * x))
                    return d * v * _beta;

                // Pomalsi logaritmicky test
                if (Math.Log(u) < 0.5 * x * x + d * (1.0 - v + Math.Log(v)))
                    return d * v * _beta;
            }
        }

        /// <summary>
        /// Generuje hodnotu zo standardneho normalneho rozdelenia N(0,1) pomocou Box-Muller transformacie.
        /// </summary>
        private double GenerateStandardNormal()
        {
            double u1 = 1.0 - _random.NextDouble();
            double u2 = 1.0 - _random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }
    }
}