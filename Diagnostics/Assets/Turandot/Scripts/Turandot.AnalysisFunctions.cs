using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Turandot
{
    public class AnalysisFunctions
    {
        public static string Evaluate(string fcn, List<string> trialData)
        {
            string result = "";
            switch (fcn)
            {
                case "BestTinnitusMasker":
                    result = BestTinnitusMasker(trialData).ToString();
                    break;
            }

            return result;
        }

        public static float BestTinnitusMasker(List<string> trialData)
        {
            float minLevel = float.PositiveInfinity;
            float cf_of_min = float.NaN;

            foreach (string t in trialData)
            {
                var r = Regex.Match(t, "\"Level\":([-\\d\\.\\s]+)");
                if (r.Success)
                {
                    float level = float.Parse(r.Groups[1].Value);
                    if (level < minLevel)
                    {
                        var m = Regex.Match(t, "\"CF_Hz\":([-\\d\\.\\s]+)");
                        if (m.Success)
                        {
                            minLevel = level;
                            cf_of_min = float.Parse(m.Groups[1].Value);
                        }
                    }
                }
            }

            return cf_of_min;
        }
    }
}