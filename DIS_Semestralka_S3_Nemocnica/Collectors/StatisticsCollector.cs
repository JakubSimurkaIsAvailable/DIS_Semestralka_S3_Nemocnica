using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public StatisticsCollector() 
        { 
            ValueCounter = 0;
            Average = 0;
            SumOfSquares = 0;
        }

        public void AddValue(double value)
        {
            SumOfSquares += value * value;
            Average = (Average * ValueCounter + value) / (ValueCounter + 1);
            ValueCounter++;
            
        }

        public (double Lower, double Upper)? GetConfidenceInterval(double z = 1.645)
        {
            if (ValueCounter < 30)
                return null; // malo dat

            double margin = z * StandardDeviation / Math.Sqrt(ValueCounter);
            return (Average - margin, Average + margin);
        }
    }
}
