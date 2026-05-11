using System;

namespace DIS_Semestralka_S3_Nemocnica.Collectors
{
    public class StatisticsCollector
    {
        public int ValueCounter { get; private set; }
        public double Average { get; private set; }
        public double SumOfSquares { get; private set; }

        public double Variance
        {
            get
            {
                if (ValueCounter < 2) return 0;
                return SumOfSquares / ValueCounter - Average * Average;
            }
        }

        public double StandardDeviation => Math.Sqrt(Variance);

        public void AddValue(double value)
        {
            SumOfSquares += value * value;
            Average = (Average * ValueCounter + value) / (ValueCounter + 1);
            ValueCounter++;
        }

        public (double Lower, double Upper)? GetConfidenceInterval(double z = 1.96)
        {
            if (ValueCounter < 30) return null;
            double margin = z * StandardDeviation / Math.Sqrt(ValueCounter);
            return (Average - margin, Average + margin);
        }
    }
}
