using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using ProtoBuf;

using KLib;
using Turandot.Schedules;

namespace Turandot.Optimizations
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class AnnealAFC : Optimization
    {
        internal class VarState
        {
            public string name;
            public float min;
            public float max;
            public float range;
            public float center;
            public float overallBest;
            public float currentBest;
            public List<float> values;
            // TURANDOT FIX 
            //private GaussianRandom _rng;

            //public VarState(string name, float min, float max, float temp, GaussianRandom rng)
            //{
            //    var s = name.Split('.');
            //    if (s.Length > 1) name = s[1];

            //    //_rng = rng;

            //    this.name = name;
            //    this.min = min;
            //    this.max = max;
            //    range = (max - min);
            //    center = (max + min) / 2f;
            //    overallBest = GenerateNewValue(center, temp);
            //    currentBest = overallBest;

            //    values = new List<float>();
            //}

            public float NewValue(float temp)
            {
                float newVal = GenerateNewValue(currentBest, temp);
                values.Add(newVal);
                return newVal;
            }

            private float GenerateNewValue(float center, float temp)
            {
                float newVal = float.NegativeInfinity;
                float delta = temp * range;
                while (newVal < min || newVal > max)
                {
                    //newVal = currentBest + delta * _rng.Next();
                }

                return newVal;
            }

            public float LastValue()
            {
                return values[values.Count - 1];
            }

            public void SwitchOverallBest()
            {
                overallBest = LastValue();
            }

            public void UpdateCurrentBest()
            {
                currentBest = overallBest;
            }

        }

        public int numTemp = 5;
        public int updateTempAfterTrials = 8;
        public int numRepsPerTest = 2;
        public float coolRate = 0.6f;

        private int _itest = 0;
        private int _irep = 0;
        private int _numTestValues = 0;
        private float _temp;
        private int _switchCriterion;
        private int[] _timesTestSelected;
        private List<VarState> _vars = new List<VarState>();
        // TURANDOT FIX 
        //private GaussianRandom _rng = new GaussianRandom();

        public override void Initialize()
        {
            _itest = 0;
            _irep = 0;
            _numTestValues = numTemp * updateTempAfterTrials;
            _timesTestSelected = new int[_numTestValues];

            _switchCriterion = Mathf.RoundToInt(0.5f * numRepsPerTest);

            _temp = coolRate;
            foreach (var v in variables)
            {
                float[] minmax = Expressions.Evaluate(v.expression);
                //_vars.Add(new VarState(v.property, KMath.Min(minmax), KMath.Max(minmax), _temp, _rng));
            }

            _maxNumberOfTrials = numTemp * updateTempAfterTrials * numRepsPerTest;

            base.Initialize();
        }

        public override SCLElement InitTrial()
        {
            SCLElement sc = new SCLElement(0, 0);
            string state = variables[0].state;
            string chan = variables[0].chan;


            int[] iorder = KMath.Permute(2);
            sc.propValPairs.Add(new PropValPair(state + ".---.TestInterval", iorder[0] + 1));

            for (int k=0; k<variables.Count; k++)
            {
                sc.propValPairs.Add(new PropValPair(variables[k].state + ".Ref1." + variables[k].property, _vars[k].overallBest));
                if (_irep == 0) _vars[k].NewValue(_temp);
                sc.propValPairs.Add(new PropValPair(variables[k].state + ".Test." + variables[k].property, _vars[k].LastValue()));
            }

            foreach (var v in nonoptVar)
            {
                float val = KLib.Expressions.EvaluateToFloatScalar(v.expression);
                sc.propValPairs.Add(new PropValPair(v.PropertyName, val));
            }

            AddLog("--> Rep #" + _irep);
            foreach (var pvp in sc.propValPairs) AddLog(pvp.ToString()); ;

            return sc;
        }

        public override void ProcessResult(string response)
        {
            string pattern = @"outcome=""([a-zA-Z]+)"";";
            Match m = Regex.Match(response, pattern);
            if (m.Success) response = m.Groups[1].Value;

            // in this context, "correct" means the test stimulus (i.e. non-best stimulus) was chosen
            bool isCorrect = _synonymsForCorrect.Contains(response.ToLower());

            if (isCorrect) _timesTestSelected[_itest]++;
            AddLog("Selected " + (isCorrect ? "test" : "ref"));

            if (++_irep == numRepsPerTest)
            {
                _irep = 0;

                if (_timesTestSelected[_itest] > _switchCriterion || (_timesTestSelected[_itest]==_switchCriterion && UnityEngine.Random.Range(0f, 1f) > 0.5f))
                {
                    AddLog("*** New best ***");
                    foreach (var v in _vars) v.SwitchOverallBest();
                }

                _itest++;
                if (_itest == _numTestValues)
                {
                    AddLog(" ");
                    foreach (var v in _vars) AddLog(storeAs + "." + v.name + " = " + v.overallBest);
                    _isFinished = true;
                }
                else if (_itest % updateTempAfterTrials == 0)
                {
                    foreach (var v in _vars) v.UpdateCurrentBest();
                    _temp *= coolRate;
                    AddLog("--> Lower temperature: " + _temp);
                }
            }
        }

        public override string ToJSONString()
        {
            string json = "{\"" + storeAs + "\":{";

            for (int k=0; k<_vars.Count; k++)
            {
                json += "\"" + _vars[k].name + "\":" + _vars[k].overallBest.ToString() + (k<_vars.Count-1 ? "," : "");
            }
            json += "}}";
            return json;
        }

        public override List<Parameter> Best()
        {
            List<Parameter> best = new List<Parameter>();
            foreach (var v in _vars) best.Add(new Parameter(v.name, v.overallBest));

            return best;
        }

#if KDEBUG
        public override string Simulate()
        {
            float[] target = new float[] { 3350, 0.8f };

            float d2test = 0;
            float d2ref = 0;
            for (int k=0; k<_vars.Count; k++)
            {
                float delta = Mathf.Abs(target[k] - _vars[k].LastValue()) / _vars[k].range;
                d2test += delta * delta;
                delta = Mathf.Abs(target[k] - _vars[k].overallBest) / _vars[k].range;
                d2ref += delta * delta;
            }

            return d2test < d2ref ? "correct" : "wrong";
        }

#endif
    }
}