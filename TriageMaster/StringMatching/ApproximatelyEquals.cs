using System;
using System.Collections.Generic;
using System.Linq;
using TriageMaster.StringMatching;

namespace TriageMaster.StringMatching
{
    public static partial class ComparisonMetrics
    {
        public static int ApproximatelyEquals(this string source, string target, params FuzzyStringComparisonOptions[] options)
        {
            int matchPercentage = 0;
            try
            {
                List<double> comparisonResults = new List<double>();

                if (!options.Contains(FuzzyStringComparisonOptions.CaseSensitive))
                {
                    source = source.ToUpper();
                    target = target.ToUpper();
                }

                // Min: 0    Max: source.Length = target.Length
                if (options.Contains(FuzzyStringComparisonOptions.UseHammingDistance))
                {
                    if (source.Length == target.Length)
                    {
                        comparisonResults.Add(source.HammingDistance(target) / target.Length);
                    }
                }

                // Min: 0    Max: 1
                if (options.Contains(FuzzyStringComparisonOptions.UseJaccardDistance))
                {
                    comparisonResults.Add(source.JaccardDistance(target));
                }

                if (options.Contains(FuzzyStringComparisonOptions.UseLongestCommonSubsequence))
                {
                    comparisonResults.Add(1 - Convert.ToDouble((source.LongestCommonSubsequence(target).Length) / Convert.ToDouble(Math.Min(source.Length, target.Length))));
                }

                if (options.Contains(FuzzyStringComparisonOptions.UseLongestCommonSubstring))
                {
                    comparisonResults.Add(1 - Convert.ToDouble((source.LongestCommonSubstring(target).Length) / Convert.ToDouble(Math.Min(source.Length, target.Length))));
                }

                if (comparisonResults.Count == 0)
                {
                    //return false;
                    return matchPercentage;
                }
                else
                {
                    matchPercentage = Convert.ToInt32((100 - (comparisonResults.Average()*100)));
                    return matchPercentage;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
