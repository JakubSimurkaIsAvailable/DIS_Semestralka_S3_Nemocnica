using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIS_Semestralka_S3_Nemocnica.Collectors
{
    public class WeightedStatisticsCollector
    {
        public double WeightedAverage => TotalWeight > 0 ? _weightedSum / TotalWeight : 0;

        public double TotalWeight { get; private set; }

        private double _lastTime;
        private double _lastValue;
        private double _weightedSum;
        private bool _initialized;

        public WeightedStatisticsCollector()
        {
            _lastTime = 0;
            _weightedSum = 0;
            TotalWeight = 0;
            _initialized = false;
        }
        /// <summary>
        /// Metoda vytvorena pomocou AI, zdokumentovana v kapitole 1.2.1
        /// </summary>
        /// <param name="value"></param>
        /// <param name="currentTime"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AddWeightedValue(double value, double currentTime)
        {
            if (currentTime < _lastTime)
            {
                throw new ArgumentException("Current time cannot be less than the last recorded time.");
            }

            if (_initialized)
            {
                double deltaTime = currentTime - _lastTime;

                if (deltaTime > 0)
                {
                    _weightedSum += _lastValue * deltaTime;
                    TotalWeight += deltaTime;
                }
            }
            else
            {
                _initialized = true;
            }

            _lastTime = currentTime;
            _lastValue = value;
        }
    }
}
