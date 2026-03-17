using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;

namespace KLib.Expressions
{
    //public class PropVal
    //{
    //    public string prop;
    //    public float value;
    //    public PropVal(string prop, float value)
    //    {
    //        this.prop = prop;
    //        this.value = value;
    //    }
    //    override public string ToString()
    //    {
    //        return prop + " = " + value;
    //    }
    //}

    // =========================================================================
    // ExpressionContext
    // =========================================================================
    // Bundles all the runtime data an expression can reference.
    // Passed into the parser on each call — no global mutable state.
    //
    // The Expressions wrapper class (Session 4) will populate a default context
    // from its static fields so all existing call sites continue to work.

    public class ExpressionContext
    {
        /// <summary>Subject-specific named values. Key = metric name, Value = numeric string.</summary>
        public SerializeableDictionary<string> Metrics { get; set; } = new SerializeableDictionary<string>();
//        public Dictionary<string, string> Metrics { get; set; } = new Dictionary<string, string>();

        /// <summary>Hearing threshold audiogram. Referenced by THR() and DR().</summary>
        public Audiograms.AudiogramData Audiogram { get; set; }

        /// <summary>Uncomfortable loudness levels. Referenced by LDL() and DR().</summary>
        public Audiograms.AudiogramData LDL { get; set; }

        /// <summary>Per-evaluation named numeric substitutions (channel properties, etc.).</summary>
        public List<Expressions.PropVal> PropVals { get; set; }
    }

    // =========================================================================
    // FunctionRegistry
    // =========================================================================
    // Maps function names to implementations.
    // Each function receives its already-evaluated arguments as float[][] and
    // returns float[]. Arity and type checking happen inside each implementation.
    //
    // New functions can be registered at startup without touching the parser.

    public class FunctionRegistry
    {
        public delegate float[] FunctionImpl(float[][] args);

        private readonly Dictionary<string, FunctionImpl> _functions
            = new Dictionary<string, FunctionImpl>(StringComparer.Ordinal);

        public void Register(string name, FunctionImpl impl) => _functions[name] = impl;
        public bool Contains(string name)                    => _functions.ContainsKey(name);

        public bool TryCall(string name, float[][] args, out float[] result)
        {
            if (_functions.TryGetValue(name, out var impl))
            {
                result = impl(args);
                return true;
            }
            result = null;
            return false;
        }

        // ------------------------------------------------------------------
        // Default registry: all built-in functions
        // ------------------------------------------------------------------

        public static FunctionRegistry CreateDefault()
        {
            var r = new FunctionRegistry();

            // linspace(start, stop, n)
            // n evenly spaced values from start to stop, inclusive on both ends.
            // This is the standard MATLAB linspace behaviour.
            r.Register("linspace", args =>
            {
                RequireArgs("linspace", args, 3);
                float start = Scalar(args[0], "linspace", "start");
                float stop  = Scalar(args[1], "linspace", "stop");
                int   n     = (int)Scalar(args[2], "linspace", "n");
                if (n < 1) throw new ExpressionException("linspace: n must be >= 1");
                if (n == 1) return new[] { start };
                float step = (stop - start) / (n - 1);
                var result = new float[n];
                for (int k = 0; k < n; k++) result[k] = start + k * step;
                return result;
            });

            // sqrt(x) — element-wise, so sqrt([4 9 16]) works too
            r.Register("sqrt", args =>
            {
                RequireArgs("sqrt", args, 1);
                return args[0].Select(v => (float)Mathf.Sqrt(v)).ToArray();
            });

            // unifrnd(min, max) — one uniform random draw
            r.Register("unifrnd", args =>
            {
                RequireArgs("unifrnd", args, 2);
                float min = Scalar(args[0], "unifrnd", "min");
                float max = Scalar(args[1], "unifrnd", "max");
                return new[] { min + (float)_rng.NextDouble() * (max - min) };
            });

            // exprndt(min, max, lambda) — truncated exponential draw
            r.Register("exprndt", args =>
            {
                RequireArgs("exprndt", args, 3);
                float min    = Scalar(args[0], "exprndt", "min");
                float max    = Scalar(args[1], "exprndt", "max");
                float lambda = Scalar(args[2], "exprndt", "lambda");
                float rn = (float)_rng.NextDouble();
                float a  = 1f - (float)Mathf.Exp(-(max - min) / lambda);
                float b  = (float)Mathf.Log(1f - a * rn);
                return new[] { min - b * lambda };
            });

            // Aggregation functions — all reduce a vector to a scalar
            r.Register("max",  args => { RequireArgs("max",  args, 1); return new[] { args[0].Max() }; });
            r.Register("min",  args => { RequireArgs("min",  args, 1); return new[] { args[0].Min() }; });
            r.Register("mean", args => { RequireArgs("mean", args, 1); return new[] { args[0].Average() }; });

            r.Register("median", args =>
            {
                RequireArgs("median", args, 1);
                var sorted = args[0].OrderBy(v => v).ToArray();
                int n = sorted.Length;
                return new[] { n % 2 == 1
                    ? sorted[n / 2]
                    : (sorted[n / 2 - 1] + sorted[n / 2]) / 2f };
            });

            // perm — Fisher-Yates shuffle
            r.Register("perm", args =>
            {
                RequireArgs("perm", args, 1);
                var result = args[0].ToArray();
                for (int i = result.Length - 1; i > 0; i--)
                {
                    int j = _rng.Next(i + 1);
                    (result[i], result[j]) = (result[j], result[i]);
                }
                return result;
            });

            // unique — sorted unique values
            r.Register("unique", args =>
            {
                RequireArgs("unique", args, 1);
                return args[0].Distinct().OrderBy(v => v).ToArray();
            });

            return r;
        }

        // Shared RNG — one instance per process, not re-seeded between calls
        private static readonly System.Random _rng = new System.Random();

        // ------------------------------------------------------------------
        // Helpers used by registrations above
        // ------------------------------------------------------------------

        private static void RequireArgs(string name, float[][] args, int count)
        {
            if (args.Length != count)
                throw new ExpressionException(
                    $"{name}() requires {count} argument{(count == 1 ? "" : "s")}, got {args.Length}");
        }

        private static float Scalar(float[] arg, string func, string argName)
        {
            if (arg.Length != 1)
                throw new ExpressionException(
                    $"{func}(): argument '{argName}' must be a scalar, got a {arg.Length}-element vector");
            return arg[0];
        }
    }

    // =========================================================================
    // ExpressionParser
    // =========================================================================
    // Recursive descent parser. Consumes List<Token> from the Tokenizer and
    // evaluates directly to float[] — no intermediate AST.
    //
    // Grammar (precedence low → high):
    //
    //   expression     := range_expr
    //   range_expr     := additive (':' additive (':' additive)?)?
    //   additive       := multiplicative (('+' | '-') multiplicative)*
    //   multiplicative := power (('*' | '/') power)*
    //   power          := unary ('^' unary)*          right-associative
    //   unary          := '-' unary | primary
    //   primary        := NUMBER
    //                   | 'inf'
    //                   | '(' expression ')'
    //                   | '[' vector_elements ']'
    //                   | IDENTIFIER '(' arg_list ')'
    //                   | IDENTIFIER                   (PropVal lookup)

    public class ExpressionParser
    {
        private List<Token> _tokens;
        private int _pos;
        private ExpressionContext _context;
        private FunctionRegistry _functions;

        // ------------------------------------------------------------------
        // Public entry point
        // ------------------------------------------------------------------

        /// <summary>
        /// Parses and evaluates <paramref name="expression"/>, returning a float[].
        /// Pass null for <paramref name="context"/> or <paramref name="functions"/>
        /// to use empty defaults.
        /// </summary>
        public float[] Parse(string expression, ExpressionContext context, FunctionRegistry functions)
        {
            if (string.IsNullOrEmpty(expression))
                return Array.Empty<float>();

            _context   = context  ?? new ExpressionContext();
            _functions = functions ?? FunctionRegistry.CreateDefault();

            // Pre-pass: expand "3x40" repeat syntax to "40 40 40"
            // This is kept as a pre-pass rather than a grammar production because
            // it doesn't compose with arithmetic — it's strictly a shorthand for
            // repeated literal values inside vector literals.
            expression = ExpandRepeats(expression);

            _tokens = new Tokenizer().Tokenize(expression);
            _pos    = 0;

            var result = ParseExpression();

            // After evaluating, we should be at EOF. If not, something is wrong
            // (e.g. "1 2" — two numbers with no operator between them outside a vector).
            if (!Check(TokenType.EOF))
                throw new ExpressionException(
                    $"Unexpected token '{Peek().Text}' at position {Peek().Position}");

            return result;
        }

        // ------------------------------------------------------------------
        // Pre-pass: repeat expansion
        // ------------------------------------------------------------------

        private static string ExpandRepeats(string expression)
        {
            // Matches "3x40" or "3x4.5" and expands to space-separated copies.
            // The leading digit(s) are the count; the value after 'x' is the literal.
            // Note: "x" here is the letter x used as a repeat operator, not multiplication.
            return Regex.Replace(expression, @"(\d+)x([\d.]+)", m =>
            {
                int    n   = int.Parse(m.Groups[1].Value);
                string val = m.Groups[2].Value;
                return string.Join(" ", Enumerable.Repeat(val, n));
            });
        }

        // ------------------------------------------------------------------
        // Grammar rules
        // ------------------------------------------------------------------

        // Top-level rule — dispatches to range handling.
        private float[] ParseExpression() => ParseRangeExpr();

        // range_expr := additive (':' additive (':' additive)?)?
        //
        // The range operator ':' sits below all arithmetic in precedence.
        // This means both sides of ':' are fully evaluated as arithmetic first:
        //
        //   THR(L,1000)+5 : LDL(L,1000)-5
        //   evaluates THR(L,1000)+5 and LDL(L,1000)-5 as the start and stop.
        //
        //   a:b      → [a, a+1, a+2, ..., b]   (step defaults to 1)
        //   a:step:b → [a, a+step, ..., b]
        private float[] ParseRangeExpr()
        {
            var first = ParseAdditive();

            if (!Check(TokenType.Colon))
                return first;

            Advance();                     // consume first ':'
            var second = ParseAdditive();

            if (!Check(TokenType.Colon))
                return BuildRange(first, new[] { 1f }, second);    // a:b form

            Advance();                     // consume second ':'
            var third = ParseAdditive();
            return BuildRange(first, second, third);               // a:step:b form
        }

        // additive := multiplicative (('+' | '-') multiplicative)*
        private float[] ParseAdditive()
        {
            var left = ParseMultiplicative();

            while (Check(TokenType.Plus) || Check(TokenType.Minus))
            {
                var op    = Advance().Type;
                var right = ParseMultiplicative();
                left = ApplyBinaryOp(left, right, op);
            }

            return left;
        }

        // multiplicative := power (('*' | '/') power)*
        private float[] ParseMultiplicative()
        {
            var left = ParsePower();

            while (Check(TokenType.Star) || Check(TokenType.Slash))
            {
                var op    = Advance().Type;
                var right = ParsePower();
                left = ApplyBinaryOp(left, right, op);
            }

            return left;
        }

        // power := unary ('^' unary)*
        //
        // Right-associative: 2^3^2 = 2^(3^2) = 512, matching MATLAB behaviour.
        // Implemented via right-recursion rather than a loop.
        private float[] ParsePower()
        {
            var left = ParseUnary();

            if (Check(TokenType.Caret))
            {
                Advance();               // consume '^'
                var right = ParsePower(); // right-recursive → right-associative
                return ApplyBinaryOp(left, right, TokenType.Caret);
            }

            return left;
        }

        // unary := '-' unary | primary
        private float[] ParseUnary()
        {
            if (Check(TokenType.Minus))
            {
                Advance();
                var operand = ParseUnary();
                return operand.Select(v => -v).ToArray();
            }

            return ParsePrimary();
        }

        // primary := NUMBER | 'inf' | '(' expr ')' | '[' ... ']' | IDENTIFIER ...
        private float[] ParsePrimary()
        {
            var token = Peek();

            switch (token.Type)
            {
                case TokenType.Number:
                    Advance();
                    return new[] { float.Parse(token.Text, CultureInfo.InvariantCulture) };

                case TokenType.LParen:
                    Advance();
                    var inner = ParseExpression();
                    Consume(TokenType.RParen, "closing ')'");
                    return inner;

                case TokenType.LBracket:
                    return ParseVectorLiteral();

                case TokenType.Identifier:
                    return ParseIdentifierOrCall();

                default:
                    throw new ExpressionException(
                        $"Expected a value but found '{token.Text}' at position {token.Position}");
            }
        }

        // ------------------------------------------------------------------
        // Identifier / function call dispatch
        // ------------------------------------------------------------------

        private float[] ParseIdentifierOrCall()
        {
            var token = Advance();        // consume the identifier
            string name = token.Text;

            if (Check(TokenType.LParen))
                return CallFunction(name, token.Position);

            // Not followed by '(' — must be 'inf' or a PropVal name.

            if (name.Equals("inf", StringComparison.OrdinalIgnoreCase))
                return new[] { float.PositiveInfinity };

            if (_context.PropVals != null)
            {
                var pv = _context.PropVals.FirstOrDefault(
                    p => string.Equals(p.prop, name, StringComparison.Ordinal));
                if (pv != null)
                    return new[] { pv.value };
            }

            throw new ExpressionException(
                $"Unknown identifier '{name}' at position {token.Position}");
        }

        // Dispatch a function call. M, THR, LDL, DR are handled specially because
        // their argument lists contain non-numeric tokens (metric names, ear codes).
        // Everything else uses standard comma-separated expression arguments.
        private float[] CallFunction(string name, int position)
        {
            switch (name)
            {
                case "M":   return CallM(position);
                case "THR": return CallAudiogramFunc(_context.Audiogram, "THR", position);
                case "LDL": return CallAudiogramFunc(_context.LDL,       "LDL", position);
                case "DR":  return CallDR(position);
            }

            // Standard call: evaluate each comma-separated argument as a full expression.
            Consume(TokenType.LParen, $"'(' after '{name}'");
            var args = new List<float[]>();

            if (!Check(TokenType.RParen))
            {
                args.Add(ParseExpression());
                while (Check(TokenType.Comma))
                {
                    Advance();
                    args.Add(ParseExpression());
                }
            }

            Consume(TokenType.RParen, $"closing ')' for '{name}'");

            if (_functions.TryCall(name, args.ToArray(), out var result))
                return result;

            throw new ExpressionException($"Unknown function '{name}' at position {position}");
        }

        // M(metricName) or M(metricName, defaultValue)
        //
        // The metric name is a raw identifier token — not evaluated as an expression.
        // The optional default IS a full expression so M(TinnitusCF, THR(L,1000)) works.
        private float[] CallM(int position)
        {
            Consume(TokenType.LParen, "'(' after M");
            var nameToken  = Consume(TokenType.Identifier, "metric name");
            string metricName = nameToken.Text.Trim();

            float defaultValue = float.NaN;
            bool  hasDefault   = false;

            if (Check(TokenType.Comma))
            {
                Advance();
                var defArr = ParseExpression();
                if (defArr.Length != 1)
                    throw new ExpressionException(
                        $"M() default value must be a scalar at position {position}");
                defaultValue = defArr[0];
                hasDefault   = true;
            }

            Consume(TokenType.RParen, "closing ')' for M");

            if (_context.Metrics != null &&
                _context.Metrics.TryGetValue(metricName, out string metricStr))
            {
                if (float.TryParse(metricStr.Trim(), NumberStyles.Float,
                                   CultureInfo.InvariantCulture, out float v))
                    return new[] { v };

                throw new ExpressionException(
                    $"Metric '{metricName}' has non-numeric value '{metricStr}'");
            }

            if (hasDefault)
                return new[] { defaultValue };

            throw new ExpressionException($"Metric '{metricName}' not found");
        }

        // THR(ear, freq) or LDL(ear, freq)
        //
        // ear  — a bare identifier: L, R, B, 1, 2, or 3
        // freq — a full expression (may be a calculation like 500*2)
        //
        // Accepts either the THR audiogram or the LDL data depending on which
        // AudiogramData object is passed in. DR() reuses this twice.
        private float[] CallAudiogramFunc(Audiograms.AudiogramData data, string funcName, int position)
        {
            Consume(TokenType.LParen, $"'(' after {funcName}");
            var earToken = Consume(TokenType.Identifier, $"ear code (L/R/B) in {funcName}()");
            Consume(TokenType.Comma, $"',' in {funcName}()");
            var freqArr = ParseExpression();    // freq can be an expression
            Consume(TokenType.RParen, $"closing ')' for {funcName}");

            if (data == null)
                throw new ExpressionException(
                    $"{funcName}() was called but the required audiogram data is not set");
            if (freqArr.Length != 1)
                throw new ExpressionException($"{funcName}() frequency must be a scalar");

            return new[] { GetEarValue(data, earToken.Text, freqArr[0], funcName) };
        }

        // DR(ear, freq) = LDL - THR
        private float[] CallDR(int position)
        {
            Consume(TokenType.LParen, "'(' after DR");
            var earToken = Consume(TokenType.Identifier, "ear code in DR()");
            Consume(TokenType.Comma, "',' in DR()");
            var freqArr = ParseExpression();
            Consume(TokenType.RParen, "closing ')' for DR");

            if (_context.Audiogram == null || _context.LDL == null)
                throw new ExpressionException(
                    "DR() requires both Audiogram and LDL data to be set in the context");
            if (freqArr.Length != 1)
                throw new ExpressionException("DR() frequency must be a scalar");

            float freq = freqArr[0];
            float thr  = GetEarValue(_context.Audiogram, earToken.Text, freq, "THR");
            float ldl  = GetEarValue(_context.LDL,       earToken.Text, freq, "LDL");
            return new[] { ldl - thr };
        }

        // Look up a threshold for one ear at one frequency.
        // Also fixes the old code's bug where the 'B' (bilateral) case averaged
        // Right+Right instead of Left+Right.
        private float GetEarValue(Audiograms.AudiogramData data, string ear, float freq, string funcName)
        {
            switch (ear.ToUpper())
            {
                case "L": case "1":
                    return data.Get(Audiograms.Ear.Left).GetThreshold(freq);

                case "R": case "2":
                    return data.Get(Audiograms.Ear.Right).GetThreshold(freq);

                case "B": case "3":
                    // Corrected from old code: was Right+Right, now Left+Right
                    return 0.5f * (data.Get(Audiograms.Ear.Left).GetThreshold(freq)
                                 + data.Get(Audiograms.Ear.Right).GetThreshold(freq));

                default:
                    throw new ExpressionException(
                        $"{funcName}(): unknown ear code '{ear}' — expected L, R, B, 1, 2, or 3");
            }
        }

        // ------------------------------------------------------------------
        // Vector literal
        // ------------------------------------------------------------------

        // Parses: '[' element* ']'
        // where element := additive ','?
        //
        // Elements can be separated by commas OR by whitespace (implicit separator).
        // Implicit separation works because ParseAdditive stops when it sees a token
        // that cannot be an operator, so two consecutive parseable values are
        // naturally treated as separate elements without any extra logic.
        //
        // Each element can itself be a vector (e.g. a range or a function call
        // returning multiple values) — all results are flattened into one array.
        //
        // Limitation: [ 1 -2 3 ] is ambiguous. The '-' reads as binary minus,
        // giving [ -1, 3 ] rather than [ 1, -2, 3 ]. Use commas when negative
        // values appear in vector literals: [ 1, -2, 3 ].
        private float[] ParseVectorLiteral()
        {
            Consume(TokenType.LBracket, "'['");
            var elements = new List<float[]>();

            while (!Check(TokenType.RBracket) && !Check(TokenType.EOF))
            {
                // Use ParseExpression (not ParseAdditive) so that range expressions
                // like [0:2:10, 20] work correctly inside vector literals.
                elements.Add(ParseExpression());
                if (Check(TokenType.Comma))
                    Advance();             // consume optional comma
                // else: implicit separator — loop condition will check for ']'
            }

            Consume(TokenType.RBracket, "']'");

            if (elements.Count == 0)
                throw new ExpressionException("Empty vector literal []");

            return elements.SelectMany(e => e).ToArray();
        }

        // ------------------------------------------------------------------
        // Vector arithmetic
        // ------------------------------------------------------------------

        // Element-wise binary operation with scalar broadcasting.
        // Scalar op vector: the scalar is broadcast to match the vector's length.
        // Vector op vector: lengths must match.
        private static float[] ApplyBinaryOp(float[] left, float[] right, TokenType op)
        {
            if      (left.Length == 1 && right.Length > 1) left  = Broadcast(left[0],  right.Length);
            else if (right.Length == 1 && left.Length > 1) right = Broadcast(right[0], left.Length);
            else if (left.Length != right.Length)
                throw new ExpressionException(
                    $"Vector length mismatch in binary operation: {left.Length} vs {right.Length}");

            var result = new float[left.Length];
            for (int k = 0; k < left.Length; k++)
            {
                switch (op)
                {
                    case TokenType.Plus:  result[k] = left[k] + right[k];                 break;
                    case TokenType.Minus: result[k] = left[k] - right[k];                 break;
                    case TokenType.Star:  result[k] = left[k] * right[k];                 break;
                    case TokenType.Slash: result[k] = left[k] / right[k];                 break;
                    case TokenType.Caret: result[k] = (float)Mathf.Pow(left[k], right[k]); break;
                    default: throw new ExpressionException($"Unknown binary operator {op}");
                }
            }
            return result;
        }

        private static float[] Broadcast(float value, int length)
        {
            var result = new float[length];
            for (int k = 0; k < length; k++) result[k] = value;
            return result;
        }

        // Build a range vector.
        // All three arguments must be scalars — the grammar ensures this because
        // ':' sits below arithmetic, so both sides are fully evaluated first.
        private static float[] BuildRange(float[] startArr, float[] stepArr, float[] stopArr)
        {
            if (startArr.Length != 1 || stepArr.Length != 1 || stopArr.Length != 1)
                throw new ExpressionException(
                    "Range bounds and step must be scalars (e.g. 1:10 or 0:2:20)");

            float start = startArr[0];
            float step  = stepArr[0];
            float stop  = stopArr[0];

            if (step == 0f)
                throw new ExpressionException("Range step cannot be zero");

            int n = (int)Mathf.Floor((stop - start) / step) + 1;

            if (n <= 0)
                throw new ExpressionException(
                    $"Range {start}:{step}:{stop} produces an empty sequence");

            var result = new float[n];
            for (int k = 0; k < n; k++)
                result[k] = start + k * step;

            return result;
        }

        // ------------------------------------------------------------------
        // Token helpers
        // ------------------------------------------------------------------

        private Token Peek()     => _tokens[_pos];
        private Token Advance()  => _tokens[_pos++];
        private bool  Check(TokenType t) => _tokens[_pos].Type == t;

        // Consume asserts the next token is of the expected type.
        // The 'expected' string is used only in the error message — write it
        // as a description of what should be here, not the type name.
        private Token Consume(TokenType type, string expected)
        {
            if (Check(type)) return Advance();

            var found = Peek();
            throw new ExpressionException(
                $"Expected {expected}, found '{found.Text}' at position {found.Position}");
        }
    }
}
