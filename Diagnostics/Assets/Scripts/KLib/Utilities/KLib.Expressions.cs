using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Rand = UnityEngine.Random;

namespace KLib
{
    public class Expressions
    {
        private readonly string operatorSymbols = "+-*/^";
        private static string _lastError = "";
        public static SerializeableDictionary<string> Metrics = new SerializeableDictionary<string>();
        public static Audiograms.AudiogramData Audiogram = null;
        public static Audiograms.AudiogramData LDL = null;
        private static System.Random _rng = null;

        public class PropVal
        {
            public string prop;
            public float value;
            public PropVal(string prop, float value)
            {
                this.prop = prop;
                this.value = value;
            }
            override public string ToString()
            {
                return prop + " = " + value;
            }
        }

        public static string LastError
        {
            get { return _lastError; }
        }

        public static string ToVectorString(int[] vals)
        {
            float[] fval = new float[vals.Length];
            for (int k = 0; k < vals.Length; k++) fval[k] = (float)vals[k];
            return ToVectorString(fval);
        }

        public static string ToVectorString(float[] vals)
        {
            if (vals.Length == 1)
            {
                return vals[0].ToString();
            }

            string s = "[";
            foreach (var v in vals)
            {
                s += " " + v.ToString();
            }
            s += " ]";

            return s;
        }

        public static string SimpleRegEx(string expression, string pattern)
        {
            string result = "";

            Match m = Regex.Match(expression, pattern);
            if (m.Success)
            {
                result = m.Groups[1].Value;
            }
            return result;
        }

        public static int SimpleRegExInt(string expression, string pattern)
        {
            return int.Parse(SimpleRegEx(expression, pattern));
        }

        public static int[] EvaluateToInt(string expression)
        {
            float[] floatVals = Evaluate(expression, null);
            int[] values = new int[floatVals.Length];
            for (int k = 0; k < floatVals.Length; k++) values[k] = (int)floatVals[k];
            return values;
        }
        public static float[] Evaluate(string expression)
        {
            return Evaluate(expression, null);
        }
        public static float[] Evaluate(string expression, List<PropVal> propVals)
        {
            Expressions e = new Expressions();
            return e.DoEvaluation(expression, propVals); 
        }

        public static int EvaluateToIntScalar(string expression)
        {
            int value = 0;
            bool result = true;
            try
            {
                float[] vals = Evaluate(expression);
                value = (int)vals[0];
            }
            catch (Exception ex)
            {
                result = false;
                _lastError = ex.Message;
            }

            return value;
        }

        public static float EvaluateToFloatScalar(string expression)
        {
            float value = float.NaN;
            bool result = true;
            try
            {
                float[] vals = Evaluate(expression);
                int rn = UnityEngine.Random.Range((int)0, (int)vals.Length);
                value = vals[rn];
            }
            catch (Exception ex)
            {
                result = false;
                _lastError = ex.Message;
            }

            return value;
        }

        public static bool TryEvaluate(string expression)
        {
            return TryEvaluate(expression, null);
        }
        public static bool TryEvaluate(string expression, out float[] values)
        {
            values = null;
            bool result = true;
            try
            {
                values = Evaluate(expression);
            }
            catch (Exception ex)
            {
                result = false;
                _lastError = ex.Message;
            }

            return result;
        }
        public static bool TryEvaluate(string expression, List<PropVal> propVals)
        {
            bool result = true;
            try
            {
                Evaluate(expression, propVals);
            }
            catch (Exception ex)
            {
                result = false;
                _lastError = ex.Message;
            }

            return result;
        }

        public static bool TryEvaluateScalar(string expression, out float value)
        {
            value = float.NaN;
            bool result = true;
            try
            {
                float[] vals = Evaluate(expression);
                value = vals[0];
            }
            catch (Exception ex)
            {
                result = false;
                _lastError = ex.Message;
            }

            return result;
        }

        public static bool EvaluateComparison(string expression)
        {
            var parts = expression.Split(new string[] { "<", ">", "==" }, StringSplitOptions.None);
            float lhs = EvaluateToFloatScalar(parts[0]);

            if (parts.Length == 1)
                return lhs > 0;

            float rhs = EvaluateToFloatScalar(parts[1]);

            if (expression.Contains("<"))
                return lhs < rhs;

            if (expression.Contains(">"))
                return lhs > rhs;

            if (expression.Contains("=="))
                return lhs == rhs;

            return false;
        }

        private float[] DoEvaluation(string expression, List<PropVal> propVals)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return new float[0];
            }

            string vectorFunction = "";

            expression = ParseVectorFunction(expression, out vectorFunction);
            expression = expression.Trim();
            expression = SubstituteMetrics(expression);
            expression = SubstitutePropVals(expression, propVals);
            expression = SubstituteAudiogram(expression);
            expression = SubstituteLDL(expression);
            expression = SubstituteDR(expression);
            expression = SubstituteFunctions(expression);
            expression = ExpandRepeats(expression);
            expression = ParenthesizeVectorSpecs(expression);

            float[] values = EvaluateString(expression);

            if (!string.IsNullOrEmpty(vectorFunction))
            {
                values = EvaluateVectorFunction(vectorFunction, values);
            }

            return values;
        }

        private string SubstituteMetrics(string expression)
        {
            string pattern = @"(M\([a-zA-Z0-9\._,\s]+\))";
            Match m = Regex.Match(expression, pattern);

            while (m.Success)
            {
                string metricName = m.Groups[1].Value;
                metricName = metricName.Substring(2, metricName.Length - 3);

                string defaultVal = "";
                var parts = metricName.Split(new char[] { ',' });
                if (parts.Length == 2)
                {
                    metricName = parts[0];
                    defaultVal = parts[1];
                }

                string metricVal = "NaN";
                if (Metrics.ContainsKey(metricName))
                {
                    metricVal = Metrics[metricName];
                }
                else if (!string.IsNullOrEmpty(defaultVal))
                    metricVal = defaultVal;
                else
                    throw new System.Exception("Metric '" + metricName + "' not found.");

                expression = expression.Replace(m.Groups[1].Value, metricVal);

                m = m.NextMatch();
            }

            return expression;
        }

        private string ParseVectorFunction(string expression, out string func)
        {
            func = "";

            expression = expression.Trim();
            if (expression.ToLower().StartsWith("unique("))
            {
                func = "unique";
            }
            else if (expression.ToLower().StartsWith("max("))
            {
                func = "max";
            }
            else if (expression.ToLower().StartsWith("mean("))
            {
                func = "mean";
            }
            else if (expression.ToLower().StartsWith("median("))
            {
                func = "median";
            }
            else if (expression.ToLower().StartsWith("min("))
            {
                func = "min";
            }
            else if (expression.ToLower().StartsWith("perm("))
            {
                func = "perm";
            }

            if (!string.IsNullOrEmpty(func))
            {
                expression = expression.Remove(expression.Length - 1);
                expression = expression.Substring(func.Length + 1);
            }

            return expression;
        }

        private float[] EvaluateVectorFunction(string func, float[] values)
        {
            float[] valuesOut = values;
            switch (func)
            {
                case "max":
                    valuesOut = new float[1];
                    valuesOut[0] = KMath.Max(values);
                    break;
                case "mean":
                    valuesOut = new float[1];
                    valuesOut[0] = KMath.Mean(values);
                    break;
                case "median":
                    valuesOut = new float[1];
                    valuesOut[0] = KMath.Median(values);
                    break;
                case "min":
                    valuesOut = new float[1];
                    valuesOut[0] = KMath.Min(values);
                    break;
                case "perm":
                    valuesOut = KMath.Permute(values);
                    break;
                case "unique":
                    valuesOut = KMath.Unique(values);
                    break;
            }
            return valuesOut;
        }

        private string SubstituteAudiogram(string expression)
        {
            string pattern = @"(THR\(([LlRrBb])\s*,\s*([0-9.]+)\))";
            Match m = Regex.Match(expression, pattern);

            if (m.Success && Audiogram == null)
            {
                throw new Exception($"Cannot evaluate '{m.Groups[0]}': no audiogram specified");
            }

            while (m.Success)
            {
                string laterality = m.Groups[2].Value;
                float freq = float.Parse(m.Groups[3].Value);
                float metricVal = float.NaN;

                switch (laterality.ToUpper())
                {
                    case "L":
                        metricVal = Audiogram.Get(Audiograms.Ear.Left).GetThreshold(freq);
                        break;

                    case "R":
                        metricVal = Audiogram.Get(Audiograms.Ear.Right).GetThreshold(freq);
                        break;

                    case "B":
                        metricVal = 0.5f * (Audiogram.Get(Audiograms.Ear.Right).GetThreshold(freq) + Audiogram.Get(Audiograms.Ear.Right).GetThreshold(freq));
                        break;
                }

                expression = expression.Replace(m.Groups[1].Value, metricVal.ToString());

                m = m.NextMatch();
            }
            return expression;
        }

        private string SubstituteLDL(string expression)
        {
            string pattern = @"(LDL\(([LlRrBb])\s*,\s*([0-9.]+)\))";
            Match m = Regex.Match(expression, pattern);

            if (m.Success && Audiogram == null)
            {
                throw new Exception($"Cannot evaluate '{m.Groups[0]}': no LDLs specified");
            }

            while (m.Success)
            {
                string laterality = m.Groups[2].Value;
                float freq = float.Parse(m.Groups[3].Value);
                float metricVal = float.NaN;

                switch (laterality.ToUpper())
                {
                    case "L":
                        metricVal = LDL.Get(Audiograms.Ear.Left).GetThreshold(freq);
                        break;

                    case "R":
                        metricVal = LDL.Get(Audiograms.Ear.Right).GetThreshold(freq);
                        break;

                    case "B":
                        metricVal = 0.5f * (LDL.Get(Audiograms.Ear.Right).GetThreshold(freq) + LDL.Get(Audiograms.Ear.Right).GetThreshold(freq));
                        break;
                }

                expression = expression.Replace(m.Groups[1].Value, metricVal.ToString());

                m = m.NextMatch();
            }

            return expression;
        }

        private string SubstituteDR(string expression)
        {
            if (Audiogram ==null || LDL == null) return expression;

            string pattern = @"(DR\(([LlRrBb])\s*,\s*([0-9.]+)\))";
            Match m = Regex.Match(expression, pattern);

            if (m.Success)
            {
                if (Audiogram == null)
                {
                    throw new Exception($"Cannot evaluate '{m.Groups[0]}': no audiogram specified");
                }
                if (LDL == null)
                {
                    throw new Exception($"Cannot evaluate '{m.Groups[0]}': no LDLs specified");
                }
            }

            while (m.Success)
            {
                string laterality = m.Groups[2].Value;
                float freq = float.Parse(m.Groups[3].Value);
                float metricVal = float.NaN;

                switch (laterality.ToUpper())
                {
                    case "L":
                        metricVal = LDL.Get(Audiograms.Ear.Left).GetThreshold(freq)
                            -Audiogram.Get(Audiograms.Ear.Left).GetThreshold(freq);
                        break;

                    case "R":
                        metricVal = LDL.Get(Audiograms.Ear.Right).GetThreshold(freq)
                            - Audiogram.Get(Audiograms.Ear.Right).GetThreshold(freq);
                        break;

                    case "B":
                        metricVal = 0.5f * (LDL.Get(Audiograms.Ear.Right).GetThreshold(freq) + LDL.Get(Audiograms.Ear.Right).GetThreshold(freq))
                            - (0.5f * (Audiogram.Get(Audiograms.Ear.Right).GetThreshold(freq) + Audiogram.Get(Audiograms.Ear.Right).GetThreshold(freq)));
                        break;
                }

                expression = expression.Replace(m.Groups[1].Value, metricVal.ToString());

                m = m.NextMatch();
            }

            return expression;
        }

        private string SubstituteFunctions(string expression)
        {
            // exprndt(min, max, lambda)
            string pattern = @"(exprndt\(\s*([\d.-]+)\s*,\s*([\d.-]+),\s*([\d.-]+)\s*\))";
            Match m = Regex.Match(expression, pattern);

            if (m.Success)
            {
                float min = float.Parse(m.Groups[2].Value);
                float max = float.Parse(m.Groups[3].Value);
                float lambda = float.Parse(m.Groups[4].Value);
                expression = expression.Replace(m.Groups[1].Value, TruncatedExponential(min, max, lambda).ToString());
                return expression;
            }

            // mean([x1 x2 x3 ...])
            pattern = @"(mean\(\s*\[([\d.\-\s]+)\s*\]\s*\))";
            m = Regex.Match(expression, pattern);
            if (m.Success)
            {
                expression = expression.Replace(m.Groups[1].Value, KMath.Mean(ParseMatrixString("[" + m.Groups[2].Value + "]")).ToString());
                return expression;
            }

            // unifrnd(min, max)
            pattern = @"(unifrnd\(\s*([\d.-]+)\s*,\s*([\d.-]+)\s*\))";
            m = Regex.Match(expression, pattern);

            if (m.Success)
            {
                float min = float.Parse(m.Groups[2].Value);
                float max = float.Parse(m.Groups[3].Value);
                expression = expression.Replace(m.Groups[1].Value, UniformRandomNumber(min, max).ToString());
                return expression;
            }

            // tnormrnd(min, max, mu, sigma)
            pattern = @"(tnormrnd\(\s*([\d.-]+)\s*,\s*([\d.-]+)\s*,\s*([\d.-]+)\s*,\s*([\d.-]+)\s*\))";
            m = Regex.Match(expression, pattern);

            if (m.Success)
            {
                float min = float.Parse(m.Groups[2].Value);
                float max = float.Parse(m.Groups[3].Value);
                float mu = float.Parse(m.Groups[4].Value);
                float sigma = float.Parse(m.Groups[5].Value);
                // TURANDOT FIX 
                //var tnr = new TruncatedNormalRandom();
                //expression = expression.Replace(m.Groups[1].Value, tnr.Next(min, max, mu, sigma).ToString());
                return expression;
            }

            // sqrt(min, max)
            pattern = @"(sqrt\(\s*([\d.]+)\s*\))";
            m = Regex.Match(expression, pattern);

            if (m.Success)
            {
                float op = float.Parse(m.Groups[2].Value);
                expression = expression.Replace(m.Groups[1].Value, Mathf.Sqrt(op).ToString());
                return expression;
            }

            return expression;
        }

        public static bool ContainsPDF(string expr)
        {
            return expr.Contains("tnormrnd") ||
                expr.Contains("unifrnd") ||
                expr.Contains("exprndt");
        }

        private string SubstitutePropVals(string expression, List<PropVal> propVals)
        {
            if (propVals == null) return expression;
            
            foreach (PropVal pv in propVals)
            {
                expression = expression.Replace(pv.prop, pv.value.ToString());
            }

            return expression;
        }

        private string ExpandRepeats(string expression)
        {
            string pattern = @"([\d]+)x([\d.-]+)";
            Match m = Regex.Match(expression, pattern);

            while (m.Success)
            {
                int n = int.Parse(m.Groups[1].Value);
                string s = "";
                for (int k = 0; k < n; k++) s += m.Groups[2].Value + " ";

                expression = expression.Replace(m.Groups[0].Value, s);

                m = m.NextMatch();
            }
            return expression;
        }

        private float TruncatedExponential(float min, float max, float lambda)
        {
            System.Random r = new System.Random();
            float rn = (float)r.NextDouble();
            //   float rn = Rand.Range(0f, 1f);

            float a = 1 - Mathf.Exp(-(max - min) / lambda);
            float b = Mathf.Log(1 - a * rn);
            return min - b * lambda;
        }

        public static float UniformRandomNumber(float min, float max)
        {
            if (_rng == null)
            {
                _rng = new System.Random();
            }
            float rn = (float)_rng.NextDouble();
            return (max - min) * rn + min;
        }

        private string ParenthesizeVectorSpecs(string s)
        {
            // Put parentheses around vector specification (A:B:C) so it parses correctly.

            int icolon = s.IndexOf(':');
            int ibracket = s.IndexOf('[');

            if (icolon < 0) return s; // No colon? No vector specification
            if (ibracket >= 0 && ibracket < icolon) return s; // vector expression already contained in brackets

            // Start at the colon and work backward to find the beginning of "A"
            int idx = icolon;
            int startIndex = 0;
            while (true)
            {
                if (s[idx] == ')') // contains an expression in parentheses...
                {
                    startIndex = FindParentheticalOpen(s, idx); // ...find the beginning
                    break;
                }
                if (operatorSymbols.IndexOf(s[idx]) >= 0 && s[idx]!='-') // operator does not belong to vector spec...
                {
                    startIndex = idx + 1; // ..."A" begins the character after the operator
                    break;
                }
                if (--idx == -1) break; // "A" begins the string
            }

            if (s.TrimStart(null)[0] == '(') // "A" beginning with (...
            {
                // ... can mean the vector spec is already in parentheses
                if (FindParentheticalClose(s, '(', startIndex) > icolon) // closing parenthesis is beyond the first colon
                    return s;
            }

            // Start at the colon and work forward
            idx = icolon;
            int endIndex = s.Length-1;
            while (true)
            {
                if (s[idx] == '(')
                {
                    idx = FindParentheticalClose(s, '(', idx);
                    break;
                }

                if (operatorSymbols.IndexOf(s[idx]) != -1)
                {
                    endIndex = idx - 1;
                    break;
                }
                if (++idx == s.Length) break;
            }

            s = s.Insert(startIndex, "(");
            s = s.Insert(endIndex + 2, ")");

            return s;
        }

        private float[] EvaluateString(string s)
        {
            List<float[]> operands = new List<float[]>();
            List<char> ops = new List<char>();

            while (s.Length > 0)
            {
                string[] split = SplitString(s);
                operands.Add(EvaluateOperand(split[0]));

                if (split.Length == 3)
                {
                    ops.Add(split[1][0]);
                    s = split[2];
                }
                else
                {
                    s = "";
                }
            }

            while (ops.Count > 0)
            {
                int index = ops.FindIndex(o => o == '^');
                if (index < 0)
                    index = ops.FindIndex(o => o == '*' || o == '/');
                if (index < 0)
                    index = ops.FindIndex(o => o == '+' || o == '-');

                if (operands[index].Length != operands[index + 1].Length)
                {
                    if (operands[index].Length == 1)
                    {
                        float v = operands[index][0];
                        int n = operands[index + 1].Length;
                        operands[index] = new float[n];
                        for (int k = 0; k < n; k++) operands[index][k] = v;
                    }
                    else if (operands[index + 1].Length == 1)
                    {
                        float v = operands[index + 1][0];
                        int n = operands[index].Length;
                        operands[index + 1] = new float[n];
                        for (int k = 0; k < n; k++) operands[index + 1][k] = v;
                    }
                    else
                        throw new Exception("Invalid expression: inconsistent vector lengths");
                }

                for (int k = 0; k < operands[index].Length; k++)
                {
                    switch (ops[index])
                    {
                        case '^':
                            operands[index][k] = Mathf.Pow(operands[index][k], operands[index + 1][k]);
                            break;
                        case '*':
                            operands[index][k] *= operands[index + 1][k];
                            break;
                        case '/':
                            operands[index][k] /= operands[index + 1][k];
                            break;
                        case '+':
                            operands[index][k] += operands[index + 1][k];
                            break;
                        case '-':
                            operands[index][k] -= operands[index + 1][k];
                            break;
                    }
                }

                operands.RemoveAt(index + 1);
                ops.RemoveAt(index);
            }

            return operands[0];
        }

        private string[] SplitString(string s)
        {
            // Find operand
            int start = 0;
            int len = -1;
            int idx = 0;

            s = s.TrimStart(null);

            if (s[0] == '(')
            {
                start = 1;
                len = FindParentheticalClose(s, '(') - 1;
                idx = len;
            }
            if (s[0] == '[')
            {
                start = 0;
                idx = FindParentheticalClose(s, '[');
                len = idx + 1;
            }

            while (true)
            {
                if (operatorSymbols.IndexOf(s[idx])!=-1 && idx > 0)
                    break;
                if (++idx == s.Length)
                    break;
            }

            if (len < 0) len = idx;

            List<string> split = new List<string>();
            split.Add(s.Substring(start, len));

            if (idx == s.Length - 1)
                throw new Exception("Invalid expression: dangling operator " + s[idx]);

            if (idx < s.Length)
            {
                split.Add(s.Substring(idx, 1));
                split.Add(s.Substring(idx + 1));
            }

            return split.ToArray();
        }

        private int FindParentheticalClose(string s, char opener, int startIndex=0)
        {
            char closer;
            switch (opener)
            {
                case '(':
                    closer = ')';
                    break;
                case '[':
                    closer = ']';
                    break;
                default:
                    throw new Exception("Invalid character");
            }

            int nopen = 1;
            int idx = startIndex + 1;
            int end = -1;
            while (true)
            {
                if (s[idx] == opener) ++nopen;
                if (s[idx] == closer) --nopen;

                if (nopen > 1 && opener == '[')
                    throw new Exception("Invalid expression: can't handle nested []'s");

                if (nopen == 0 && end < 0) end = idx;
                if (nopen <= 0 && operatorSymbols.IndexOf(s[idx])!=-1) break;
                if (++idx == s.Length) break;
            }

            if (nopen > 0)
                throw new Exception("Invalid expression: unbalanced " + opener);
            if (nopen < 0)
                throw new Exception("Invalid expression: unbalanced " + closer);

            return end;
        }

        private int FindParentheticalOpen(string s, int startIndex)
        {
            char opener = '(';
            char closer = ')';

            int nclose = 1;
            int idx = 1;
            while (true)
            {
                if (s[idx] == opener) --nclose;
                if (s[idx] == closer) ++nclose;

                if (nclose == 0) break;
                if (--idx == -1) break;
            }

            if (nclose > 0)
                throw new Exception("Invalid expression: unbalanced " + closer);

            return idx;
        }

        private float[] EvaluateOperand(string operand)
        {
            float[] result = null;
            if (operand[0] == '[')
            {
                result = ParseMatrixString(operand);
            }
            else if (operand.Contains(":"))
            {
                result = ParseVectorString(operand);
            }
            else if (ContainsOperator(operand))
            {
                result = EvaluateString(operand);
            }
            else if (operand.ToLower().StartsWith("inf"))
            {
                result = new float[1] { float.PositiveInfinity };
            }
            else if (operand.ToLower().StartsWith("-inf"))
            {
                result = new float[1] { float.NegativeInfinity };
            }
            else
            {
                result = new float[1] { float.Parse(operand) };
            }
            return result;
        }

        private bool ContainsOperator(string s)
        {
            foreach (char op in operatorSymbols)
            {
                if (s.IndexOf(op) > 0)
                    return true;
            }
            return false;
        }

        private float[] ParseMatrixString(string matrixString)
        {
            List<float[]> vals = new List<float[]>();
            
            string[] split = matrixString.Substring(1, matrixString.Length-2).Trim().Split(';', ' ');
            foreach (string s in split)
            {
                if (!string.IsNullOrEmpty(s)) vals.Add(EvaluateOperand(s));
            }

            int n = vals.Sum(v => v.Length);
            float[] result = new float[n];
            int idx = 0;
            foreach (float[] v in vals)
            {
                for (int k = 0; k < v.Length; k++) result[idx++] = v[k];
            }

            return result;
        }


        private float[] ParseVectorString(string vectorString)
        {
            //if (vectorString[0] == '(')
            //{
            //    vectorString = vectorString.Substring(1, vectorString.Length - 2);
            //}

            string[] split = vectorString.Split(new char[] { ':' });

            if (split.Length > 3)
                throw new Exception("Invalid vector expression: '" + vectorString + "'");

            float[] components = new float[split.Length];
            for (int k=0; k<split.Length; k++)
            {
                float[] v = EvaluateOperand(split[k]);
                if (v.Length > 1)
                    throw new Exception("Invalid vector expression");

                components[k] = v[0];
            }

            float start = components[0];
            float step = (components.Length == 3) ? components[1] : 1;
            float stop = (components.Length == 3) ? components[2] : components[1];

            if (step == 0)
                throw new Exception("Invalid vector expression: step size = 0");

            int n = (int)Mathf.Floor((stop - start) / step) + 1;

            if (n <= 0)
                throw new Exception("Invalid vector expression:" + vectorString);

            float[] result = new float[n];
            for (int k=0; k< n; k++)
            {
                result[k] = start + k * step;
            }

            return result;
        }

    }
}
