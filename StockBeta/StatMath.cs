using System;
using System.Collections.Generic;
using System.Linq;

namespace StockBeta
{
    class StatMath
    {
        public static double mean(List<double> values)
        {
            return values.Average();
        }

        public static double variance(List<double> values)
        {
            double variance = 0;
            double meanVal = mean(values);

            foreach (double val in values) {
                variance += Math.Pow((val - meanVal), 2);
            }
            return variance / (values.Count - 1);
        }

        public static double covariance(List<double> values1, List<double> values2)
        {
            if (values1.Count == 0 || values2.Count == 0) {
                throw new Exception("Count is 0!");
            } else if (values1.Count != values2.Count) {
                throw new Exception("Counts not equal! " + values1.Count.ToString() 
                                    + " != " + values2.Count.ToString());
            }
            double mean1 = values1.Average();
            double mean2 = values2.Average();
            double cov = 0;
            for(int i = 0; i < values1.Count; ++i) {
                cov += (values1[i] - mean1) * (values2[i] - mean2);
            }
            return cov / (values1.Count - 1);
        }
    }
}
