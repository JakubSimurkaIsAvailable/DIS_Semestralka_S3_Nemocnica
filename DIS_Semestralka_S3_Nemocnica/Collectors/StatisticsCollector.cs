using System;
using System.Collections.Generic;

namespace DIS_Semestralka_S3_Nemocnica.Collectors
{
    public class StatisticsCollector
    {
        public int ValueCounter { get; private set; }
        public double Average { get; private set; }
        public double SumOfSquares { get; private set; }

        private readonly bool _track;
        private readonly List<double>? _values;

        public double Variance
        {
            get
            {
                if (ValueCounter < 2) return 0;
                return SumOfSquares / ValueCounter - Average * Average;
            }
        }

        public double StandardDeviation => Math.Sqrt(Variance);

        public StatisticsCollector(bool track = false)
        {
            _track = track;
            _values = track ? new List<double>() : null;
        }

        public void AddValue(double value)
        {
            SumOfSquares += value * value;
            Average = (Average * ValueCounter + value) / (ValueCounter + 1);
            ValueCounter++;
            if (_track) lock (_values!) _values!.Add(value);
        }

        public double[] GetValues()
        {
            if (!_track || _values == null) return Array.Empty<double>();
            lock (_values) return _values.ToArray();
        }

        public (double Lower, double Upper)? GetConfidenceInterval(double z = 1.96)
        {
            if (ValueCounter < 30)
                return null;

            double margin = z * StandardDeviation / Math.Sqrt(ValueCounter);
            return (Average - margin, Average + margin);
        }
    }
}
