using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KLib.Expressions
{
    /// <summary>
    /// Public API for evaluating MATLAB-style numeric expressions.
    ///
    /// Expressions can reference:
    ///   - Arithmetic:        2 + 3 * (4 - 1)
    ///   - Ranges:            20:5:80   or   0:100
    ///   - Vector literals:   [1 2 3]   or   [1, 2, 3]
    ///   - Repeat shorthand:  3x40      (expands to 40 40 40 inside a vector)
    ///   - Functions:         linspace, sqrt, max, min, mean, median, perm, unique,
    ///                        unifrnd, exprndt
    ///   - Audiogram lookups: THR(L, 1000)  LDL(R, 4000)  DR(B, 2000)
    ///   - Subject metrics:   M(TinnitusCF)  or  M(TinnitusCF, 4000)  (with default)
    ///   - PropVals:          per-call named substitutions passed to Evaluate overloads
    ///
    /// Set Metrics, Audiogram, and LDL once per subject change.
    /// All Evaluate* methods are thread-safe with respect to each other provided
    /// those three fields are not mutated concurrently.
    /// </summary>
    public class Expressions
    {
        // -------------------------------------------------------------------------
        // Global context — set these when the subject changes
        // -------------------------------------------------------------------------

        /// <summary>Subject-specific named numeric values. Key = name, Value = numeric string.</summary>
        public static SerializeableDictionary<string> Metrics = new SerializeableDictionary<string>();

        /// <summary>Hearing threshold audiogram. Used by THR() and DR().</summary>
        public static Audiograms.AudiogramData Audiogram = null;

        /// <summary>Uncomfortable loudness levels. Used by LDL() and DR().</summary>
        public static Audiograms.AudiogramData LDL = null;

        /// <summary>The error message from the most recent failed Try* call.</summary>
        public static string LastError { get; private set; } = "";

        // -------------------------------------------------------------------------
        // PropVal — per-evaluation named substitution
        // -------------------------------------------------------------------------

        /// <summary>
        /// A named float value that can be substituted into an expression at
        /// evaluation time. Used for channel properties and similar per-call context.
        /// </summary>
        public class PropVal
        {
            public string prop;
            public float  value;

            public PropVal(string prop, float value)
            {
                this.prop  = prop;
                this.value = value;
            }

            public override string ToString() => prop + " = " + value;
        }

        // -------------------------------------------------------------------------
        // Shared parser infrastructure
        // -------------------------------------------------------------------------

        // FunctionRegistry is stateless after construction — one instance is enough.
        private static readonly FunctionRegistry _functions = FunctionRegistry.CreateDefault();

        // Build a fresh ExpressionContext from the current static fields.
        // Called on every evaluation so that changes to Metrics/Audiogram/LDL
        // are always picked up without any explicit cache invalidation.
        private static ExpressionContext CreateContext(List<PropVal> propVals = null)
        {
            return new ExpressionContext
            {
                Metrics   = Metrics,
                Audiogram = Audiogram,
                LDL       = LDL,
                PropVals  = propVals
            };
        }

        // Core evaluation — all public methods funnel through here.
        // ExpressionParser is cheap to construct (no setup work in the constructor).
        private static float[] RunParser(string expression, List<PropVal> propVals = null)
        {
            return new ExpressionParser().Parse(expression, CreateContext(propVals), _functions);
        }

        // -------------------------------------------------------------------------
        // Evaluate — returns float[]
        // -------------------------------------------------------------------------

        /// <summary>Evaluates the expression and returns all resulting values.</summary>
        public static float[] Evaluate(string expression)
        {
            return RunParser(expression);
        }

        /// <summary>Evaluates the expression with per-call PropVal substitutions.</summary>
        public static float[] Evaluate(string expression, List<PropVal> propVals)
        {
            return RunParser(expression, propVals);
        }

        // -------------------------------------------------------------------------
        // EvaluateToInt — convenience wrappers returning int[]
        // -------------------------------------------------------------------------

        /// <summary>Evaluates the expression and casts each result value to int.</summary>
        public static int[] EvaluateToInt(string expression)
        {
            float[] floatVals = RunParser(expression);
            int[]   values    = new int[floatVals.Length];
            for (int k = 0; k < floatVals.Length; k++)
                values[k] = (int)floatVals[k];
            return values;
        }

        // -------------------------------------------------------------------------
        // EvaluateToIntScalar — single int result
        // -------------------------------------------------------------------------

        /// <summary>
        /// Evaluates the expression and returns the first value as an int.
        /// Returns 0 and sets LastError on failure.
        /// </summary>
        public static int EvaluateToIntScalar(string expression)
        {
            try
            {
                return (int)RunParser(expression)[0];
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return 0;
            }
        }

        // -------------------------------------------------------------------------
        // EvaluateRandomElement — pick one element at random from the result vector
        // -------------------------------------------------------------------------

        /// <summary>
        /// Evaluates the expression and returns a randomly chosen element from the
        /// result vector. Use this when the expression intentionally describes a
        /// set of values and you want one draw from that set — for example, a
        /// parameter that should vary randomly across trials.
        ///
        /// For expressions that must produce a single deterministic scalar,
        /// use TryEvaluateScalar instead.
        ///
        /// Returns NaN and sets LastError on failure.
        /// </summary>
        public static float EvaluateRandomElement(string expression)
        {
            try
            {
                float[] vals = RunParser(expression);
                return vals[UnityEngine.Random.Range(0, vals.Length)];
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return float.NaN;
            }
        }

        // -------------------------------------------------------------------------
        // TryEvaluate — bool return, no exception thrown
        // -------------------------------------------------------------------------

        /// <summary>
        /// Evaluates the expression. Returns true on success, false on failure.
        /// Sets LastError on failure.
        /// Use when you only need to know whether the expression is valid.
        /// </summary>
        public static bool TryEvaluate(string expression)
        {
            return TryEvaluate(expression, (List<PropVal>)null);
        }

        /// <summary>
        /// Evaluates the expression and returns the result via <paramref name="values"/>.
        /// Returns true on success, false on failure. Sets LastError on failure.
        /// </summary>
        public static bool TryEvaluate(string expression, out float[] values)
        {
            values = null;
            try
            {
                values = RunParser(expression);
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Evaluates the expression with per-call PropVal substitutions.
        /// Returns true on success, false on failure. Sets LastError on failure.
        /// </summary>
        public static bool TryEvaluate(string expression, List<PropVal> propVals)
        {
            try
            {
                RunParser(expression, propVals);
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        // -------------------------------------------------------------------------
        // TryEvaluateScalar — single deterministic float result
        // -------------------------------------------------------------------------

        /// <summary>
        /// Evaluates the expression and returns the first element as a scalar.
        /// Returns true on success, false on failure. Sets LastError on failure.
        /// Use this when the expression is expected to produce exactly one value.
        /// </summary>
        public static bool TryEvaluateScalar(string expression, out float value)
        {
            value = float.NaN;
            try
            {
                value = RunParser(expression)[0];
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        // -------------------------------------------------------------------------
        // EvaluateComparison
        // -------------------------------------------------------------------------

        /// <summary>
        /// Evaluates a comparison expression such as "M(Age) > 60" or "level == 65".
        /// Supports operators: &lt;  &gt;  ==
        /// If no operator is present, returns true when the expression is non-zero.
        /// </summary>
        public static bool EvaluateComparison(string expression)
        {
            var parts = expression.Split(new string[] { "<", ">", "==" },
                                         StringSplitOptions.None);

            float lhs = EvaluateRandomElement(parts[0]);
            if (parts.Length == 1)
                return lhs > 0;

            float rhs = EvaluateRandomElement(parts[1]);

            if (expression.Contains("<"))  return lhs < rhs;
            if (expression.Contains(">"))  return lhs > rhs;
            if (expression.Contains("==")) return lhs == rhs;

            return false;
        }

        // -------------------------------------------------------------------------
        // ContainsPDF
        // -------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the expression contains a random-draw function
        /// (unifrnd, exprndt, tnormrnd). Used by callers that need to know whether
        /// repeated evaluation will produce different values.
        /// </summary>
        public static bool ContainsPDF(string expression)
        {
            return expression.Contains("tnormrnd") ||
                   expression.Contains("unifrnd")  ||
                   expression.Contains("exprndt");
        }

        // -------------------------------------------------------------------------
        // ToVectorString — format float[] back to expression syntax
        // -------------------------------------------------------------------------

        /// <summary>Formats an int array as an expression string (e.g. "[1 2 3]" or "42").</summary>
        public static string ToVectorString(int[] vals)
        {
            float[] fval = new float[vals.Length];
            for (int k = 0; k < vals.Length; k++) fval[k] = (float)vals[k];
            return ToVectorString(fval);
        }

        /// <summary>Formats a float array as an expression string (e.g. "[1 2 3]" or "3.14").</summary>
        public static string ToVectorString(float[] vals)
        {
            if (vals.Length == 1)
                return vals[0].ToString();

            string s = "[";
            foreach (var v in vals)
                s += " " + v.ToString();
            s += " ]";
            return s;
        }

        // -------------------------------------------------------------------------
        // SimpleRegEx — unrelated utility methods that live here for historical reasons
        // -------------------------------------------------------------------------

        /// <summary>Returns the first capture group of <paramref name="pattern"/> in
        /// <paramref name="expression"/>, or empty string if no match.</summary>
        public static string SimpleRegEx(string expression, string pattern)
        {
            var m = Regex.Match(expression, pattern);
            return m.Success ? m.Groups[1].Value : "";
        }

        /// <summary>Parses the first capture group of <paramref name="pattern"/> as int.</summary>
        public static int SimpleRegExInt(string expression, string pattern)
        {
            return int.Parse(SimpleRegEx(expression, pattern));
        }
    }
}
