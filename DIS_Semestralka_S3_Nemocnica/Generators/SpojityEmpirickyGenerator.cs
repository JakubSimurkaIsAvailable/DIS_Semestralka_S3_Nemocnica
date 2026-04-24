using DIS_Semestralka_S3_Nemocnica.Generators.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIS_Semestralka_S3_Nemocnica.Generators
{
    public class SpojityEmpirickyGenerator
    {
        List<RozdelenieSpojite> Rozdelenia = new List<RozdelenieSpojite>();
        List<double> Pravdepodobnosti = new List<double>();
        Random Random;

        public SpojityEmpirickyGenerator(Random generatorGeneratorov, List<double> minima, List<double> maxima, List<double> pravdepodobnosti)
        {
            if (minima.Count != maxima.Count || minima.Count != pravdepodobnosti.Count)
            {
                throw new ArgumentException("Vstupne zoznamy nemajú rovnaky pocet prvkov.");
            }
            Pravdepodobnosti = pravdepodobnosti;
            SkontrolujPravdepodobnosti();
            Random = new Random(generatorGeneratorov.Next());
            for (int i = 0; i < minima.Count; i++)
            {
                Rozdelenia.Add(new RozdelenieSpojite(generatorGeneratorov, minima[i], maxima[i]));
            }
        }

        private void SkontrolujPravdepodobnosti()
        {
            if (Math.Abs((Pravdepodobnosti.Sum() - 1)) > 0.00001)
            {
                throw new ArgumentException("Suma pravdepodobnosti musi byt 1.");
            }
        }

        public double Generate()
        {
            double randomValue = Random.NextDouble();
            for (int i = 0; i < Pravdepodobnosti.Count; i++)
            {
                if (randomValue < Pravdepodobnosti[i])
                {
                    return Rozdelenia[i].Generate();
                }
                randomValue -= Pravdepodobnosti[i];
            }
            return Rozdelenia[Pravdepodobnosti.Count - 1].Generate();
        }
    }
}
