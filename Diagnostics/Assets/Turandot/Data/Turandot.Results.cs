using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using KLib;

namespace Turandot
{
    public class Results
    {
        private class Result
        {
            public string family;
            public int ix;
            public int iy;
            public List<float> values = new List<float>();

            public Result(string family, int ix, int iy)
            {
                this.family = family;
                this.ix = ix;
                this.iy = iy;
            }

            public float CV
            {
                get { return KMath.CoeffVar(values.ToArray()); }
            }
        }

        List<Result> _results = new List<Result>();

        public void Clear()
        {
            _results.Clear();
        }

        public void Add(string family, int ix, int iy, string result)
        {
            float value = float.NaN;
            string pattern = @"(slider=([\d.-]+);)";
            Match m = Regex.Match(result, pattern);

            if (m.Success)
            {
                value = float.Parse(m.Groups[2].Value);
            }

            Add(family, ix, iy, value);
        }

        public void Add(string family, int ix, int iy, float value)
        {
            Result r = _results.Find(o => o.family == family && o.ix == ix && o.iy == iy);
            if (r == null)
            {
                r = new Result(family, ix, iy);
                _results.Add(r);
            }
            r.values.Add(value);
        }

        public float AverageCV
        {
            get
            {
                float cvsum = 0;
                int n = 0;
                foreach(Result r in _results)
                {
                    float cv = r.CV;
                    if (cv > 0)
                    {
                        cvsum += cv;
                        n++;
                    }
                }

                return (n > 0) ? cvsum / n : float.NaN;
            }
        }


    }
}
