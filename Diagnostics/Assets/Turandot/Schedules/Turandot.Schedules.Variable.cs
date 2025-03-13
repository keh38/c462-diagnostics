using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

using KLib;
using ExtensionMethods;

namespace Turandot.Schedules
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Variable
    {
        public string state = "";
        public string chan = "";
        public string property = "";
        public VarDimension dim = VarDimension.X;
        public string expression = "";

        [JsonIgnore]
        float[] _values;

        public Variable()
        {
        }

        public Variable(string state, string chan, string property)
        {
            this.state = state;
            this.chan = chan;
            this.property = property;
        }

        public Variable(Variable that)
        {
            this.state = that.state;
            this.chan = that.chan;
            this.property = that.property;
            this.dim = that.dim;
            this.expression = that.expression;
        }

        public float[] Values { get { return _values; } }
        public int EvaluateExpression()
        {
            return EvaluateExpression(null);
        }
        public int EvaluateExpression(List<Expressions.PropVal> propVals)
        {
            _values = Expressions.Evaluate(expression, propVals);

            return _values.Length;
        }

        public int EvaluateExpression(string xvector, string yvector)
        {
            string expr = expression;
            if (!string.IsNullOrEmpty(xvector))
            {
                if (expression.Contains("X")) expr = expr.Replace("X", xvector);
                //else if (expression.Contains("X")) expr = expr.Replace("X", xvector[0].ToString());
            }
            if (!string.IsNullOrEmpty(yvector))
            {
                if (expression.Contains("Y")) expr = expr.Replace("Y", yvector);
                //else if (expression.Contains("Y")) expr = expr.Replace("Y", yvector[0].ToString());
            }

            _values = Expressions.Evaluate(expr);

            return _values.Length;
        }

        public int EvaluateExpression(float[] xvector, float[] yvector)
        {
            string expr = expression;
            if (xvector != null && xvector.Length > 0)
            {
                if (expression.Contains("X")) expr = expr.Replace("X", Expressions.ToVectorString(xvector));
                //else if (expression.Contains("X")) expr = expr.Replace("X", xvector[0].ToString());
            }
            if (yvector != null && yvector.Length > 0)
            {
                if (expression.Contains("Y")) expr = expr.Replace("Y", Expressions.ToVectorString(yvector));
                //else if (expression.Contains("Y")) expr = expr.Replace("Y", yvector[0].ToString());
            }

            _values = Expressions.Evaluate(expr);

            return _values.Length;
        }

        public int EvaluateExpression(int N)
        {
            if (Expressions.ContainsPDF(expression))
            {
                _values = new float[N];
                for (int k = 0; k < N; k++) _values[k] = Expressions.EvaluateToFloatScalar(expression);
            }
            else
            {
                _values = Expressions.Evaluate(expression);
            }

            return _values.Length;
        }

        public string ToVectorString()
        {
            return Expressions.ToVectorString(_values);
        }

        public Variable Clone()
        {
            Variable v = new Variable();
            v.state = state;
            v.chan = chan;
            v.property = property;
            v.dim = dim;
            v.expression = expression;

            return v;
        }

        [JsonIgnore]
        public string PropertyName
        {
            get { return (string.IsNullOrEmpty(state) ? "" : state + ".") + chan + "." + property; }
        }

        [JsonIgnore]
        public int Length
        {
            get { return _values.Length; }
        }

        public float GetValue(int ix, int iy)
        {
            return GetValue(ix, iy, -1);
        }

        public float GetValue(int ix, int iy, int num)
        {
            float value = 0f;

            if (dim == VarDimension.Ind)
            {
                if (num > -1)
                {
                    int index = num % _values.Length;
                    if (index == 0)
                    {
                        _values = KLib.KMath.Permute(_values);
                    }
                    value = _values[index];
                }
                else
                {
                    value = _values.GetRandom();
                }
            }
            else if (dim == VarDimension.X)
            {
                value = _values[ix];
            }
            else if (dim == VarDimension.Y)
            {
                value = _values[iy];
            }

            return value;
        }

        public float GetValue(int index)
        {
            return _values[index];
        }

        public float GetValue()
        {
            return _values.GetRandom();
        }
    }
}