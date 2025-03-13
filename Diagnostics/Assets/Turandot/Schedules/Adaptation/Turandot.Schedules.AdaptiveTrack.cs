using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Schedules
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class AdaptiveTrack
    {
        public string name;
        public string state;
        public string chan;
        public string param;

        public float refVal;
        public float startVal;
        public float stepVal;
        public float minVal;
        public float maxVal;
        public float[] steps = new float[] { 1 };
        public int[] reversalsPerStep = new int[] { 6 };
        public AdaptStepMode stepMode = AdaptStepMode.Add;

        public TrialType trackedVarType = TrialType.CSplus;

        public int Nup = 1;
        public int Ndown = 2;

        public int Nreverse = 6;
        public AdaptComputation computation = AdaptComputation.Mean;
        public int computeLastN = 4;

        public int maxNumTrials = -1;

        public float pCatch = 0;
        public bool doAllCatches = true;
        public CatchInterval catchInterval = CatchInterval.ValueChange;
        public int startWithNcatch = 0;

        public List<Variable> variables = new List<Variable>();
        public List<Variable> catches = new List<Variable>();

        public string thresholdExpr = "A";
        public string storeThresholdAs = "";

        TrackState _state = new TrackState();

        private float[] _stepByReversalNumber;

        List<string> _synonymsForCorrect = new List<string>(new string[] { "go", "right", "correct" });

        public AdaptiveTrack()
        {
        }

        public AdaptiveTrack(string name)
        {
            this.name = name;
        }

        public bool IsResponseCorrect(string response)
        {
            bool isCorrect = false;
            
            switch (trackedVarType)
            {
                case TrialType.CSplus:
                    isCorrect = response.ToLower() == "hit";
                    break;
                case TrialType.CSminus:
                    isCorrect = response.ToLower() == "withhold";
                    break;

                default:
                    isCorrect = _synonymsForCorrect.Contains(response.ToLower());
                    break;
            }

            return isCorrect;
        }

        [JsonIgnore]
        public float[] CatchValues
        {
            get { return _state.catchValues[0]; }
        }

        [JsonIgnore]
        public float CurrentValue
        {
            get { return _state.value; }
        }

        [JsonIgnore]
        public string CurrentTrialType
        {
            get { return _state.type; }
        }

        [JsonIgnore]
        public bool IsFinished
        {
            get { return _state.isFinished; }
        }

        [JsonIgnore]
        public int NumTrials
        {
            get { return _state.history.Count; }
        }

        [JsonIgnore]
        public string PropertyName
        {
            get { return state + "." + chan + "." + param; }
        }

        [JsonIgnore]
        public float FinalValue { set; get; }

        [JsonIgnore]
        public float Threshold { set; get; }

        [JsonIgnore]
        public List<AdaptHistory> History
        {
            get { return _state.history; }
        }

        public AdaptiveTrack Clone(string name)
        {
            AdaptiveTrack t = new AdaptiveTrack(name);
            t.state = this.state;
            t.chan = this.chan;
            t.param = this.param;
            t.refVal = this.refVal;
            t.startVal = this.startVal;
            t.stepVal = this.stepVal;
            t.minVal = this.minVal;
            t.maxVal = this.maxVal;
            t.steps = this.steps;
            t.reversalsPerStep = this.reversalsPerStep;
            t.stepMode = this.stepMode;
            t.trackedVarType = this.trackedVarType;
            t.Nup = this.Nup;
            t.Ndown = this.Ndown;
            t.Nreverse = this.Nreverse;
            t.computation = this.computation;
            t.computeLastN = this.computeLastN;
            t.pCatch = this.pCatch;
            t.doAllCatches = this.doAllCatches;
            t.catchInterval = this.catchInterval;
            t.startWithNcatch = this.startWithNcatch;
            t.thresholdExpr = this.thresholdExpr;
            t.storeThresholdAs = this.storeThresholdAs;

            t.variables = new List<Variable>();
            foreach (Variable v in this.variables) t.variables.Add(v.Clone());
            t.catches = new List<Variable>();
            foreach (Variable v in this.catches) t.catches.Add(v.Clone());

            return t;
        }


        public void Initialize()
        {
            _state.Initialize(startVal, startWithNcatch);
            FinalValue = float.NaN;
            Threshold = float.NaN;

            var pv = new List<KLib.Expressions.PropVal>() { new KLib.Expressions.PropVal("AV", startVal) };
            foreach (Variable v in variables)
            {
                v.EvaluateExpression(pv);
            }
            foreach (Variable v in catches)
            {
                v.EvaluateExpression();
            }

            Nreverse = reversalsPerStep.Sum();
            _stepByReversalNumber = new float[Nreverse];

            int index = 0;
            for (int k=0; k < reversalsPerStep.Length; k++)
            {
                for (int ks=0; ks < reversalsPerStep[k]; ks++)
                {
                    _stepByReversalNumber[index] = steps[k];
                    index++;
                }
            }

            SetNextTrial(false);
        }

#if KDEBUG
        public string SimulatedResult(TrialType trialType, float threshold)
        {
            string result = "";
            if (_state.type == "test") result = _state.value < threshold ? "correct" : "wrong";
            else result = "Hit";

            return result;
        }
#endif

        public AdaptTrialResult ProcessResponse(string response)
        {
            AdaptTrialResult result = new AdaptTrialResult();
            AdaptHistory h = new AdaptHistory(_state.value, response);

            bool isCorrect = IsResponseCorrect(response);
            if (response.ToLower() == "done")
            {
                result.trackFinished = true;
                result.userStop = true;
            }
            else if (_state.type == "test" && isCorrect)
                result = ProcessAdaptCorrect();
            else if (_state.type == "test" && !isCorrect)
                result = ProcessAdaptIncorrect();
            else if (_state.type == "catch")
            {
                h.value = float.NaN;
                h.catchValue = CatchValues[0];
                result.log = "Catch response: " + response;
            }

            if (!result.outOfRange && maxNumTrials > 0 && _state.history.Count == maxNumTrials - 1)
            {
                result.outOfRange = true;
                result.trackFinished = true;
                result.log += "\nMax number of trials reached.";
                _state.isFinished = true;
            }

            h.log = result.log;
            h.reversal = result.reversal;

            _state.history.Add(h);
            return result;
        }

        AdaptTrialResult ProcessAdaptCorrect()
        {
            AdaptTrialResult result = new AdaptTrialResult();

            _state.numCorrect = _state.numCorrect > 0 ? _state.numCorrect + 1 : 1;
            result.log = _state.numCorrect + " correct";

            if (_state.numCorrect == Ndown && _state.lastDirection > 0)
            {
                ++_state.numReverse;
                result.reversal = true;
                result.log += " --> REVERSAL #" + _state.numReverse;
                result.trackFinished = _state.numReverse >= Nreverse;
            }

            if (_state.numCorrect == Ndown && !result.trackFinished)
            {
                result.valueChange = true;
                result.log += " --> Down";
                _state.numCorrect = 0;
                _state.lastDirection = -1;
                _state.lastValue = _state.value;

                float step = _stepByReversalNumber[_state.numReverse];

                if (stepMode == AdaptStepMode.Add)
                {
                    _state.value += step;
                }
                else
                {
                    _state.value *= step;
                }

                if (_state.value >= maxVal) _state.value = maxVal;
                if (_state.value < minVal) _state.value = minVal;

                result.outOfRange = _state.value <= minVal || _state.value >= maxVal;
                if (result.outOfRange)
                {
                    result.log += "\nTest value out of range.";
                    _state.isFinished = true;
                    result.trackFinished = true;
                }
            }

            return result;
        }

        AdaptTrialResult ProcessAdaptIncorrect()
        {
            AdaptTrialResult result = new AdaptTrialResult();

            _state.numCorrect = _state.numCorrect < 0 ? _state.numCorrect - 1 : -1;
            result.log = -_state.numCorrect + " wrong";

            if (_state.numCorrect == -Nup && _state.lastDirection < 0)
            {
                ++_state.numReverse;
                result.reversal = true;
                result.log += " --> REVERSAL #" + _state.numReverse;
                result.trackFinished = _state.numReverse >= Nreverse;
            }

            if (_state.numCorrect == -Nup && !result.trackFinished)
            {
                result.valueChange = true;
                result.log += " --> Up";
                _state.numCorrect = 0;
                _state.lastDirection = 1;
                _state.lastValue = _state.value;

                float step = _stepByReversalNumber[_state.numReverse];

                if (stepMode == AdaptStepMode.Add)
                {
                    _state.value -= step;
                }
                else
                {
                    _state.value /= step;
                    Debug.Log(_state.value);
                }

                if (_state.value < minVal) _state.value = minVal;
                if (_state.value > maxVal) _state.value = maxVal;

                result.outOfRange = _state.value <= minVal || _state.value >= maxVal;
                if (result.outOfRange)
                {
                    result.log += "\nTest value out of range.";
                    _state.isFinished = true;
                    result.trackFinished = true;
                }
            }
            return result;
        }
        
        public void ComputeThreshold(bool userStop)
        {
            if (userStop)
            {
                _state.finalValue = _state.history[_state.history.Count - 1].value;
            }
            else
            {
                List<AdaptHistory> reversals = _state.history.FindAll(h => h.reversal == true);
                if (Nreverse > computeLastN) reversals.RemoveRange(0, Nreverse - computeLastN);
                float[] values = reversals.Select(r => r.value).ToArray();

                if (computation == AdaptComputation.Mean)
                {
                    _state.finalValue = KLib.KMath.Mean(values);
                }
                else if (computation == AdaptComputation.Median)
                {
                    _state.finalValue = KLib.KMath.Median(values);
                }
            }

            float thr;
            string expr = thresholdExpr.Replace("A", _state.finalValue.ToString());
            KLib.Expressions.TryEvaluateScalar(expr, out thr);

            FinalValue = _state.finalValue;
            Threshold = thr;
            _state.history[_state.history.Count - 1].log += "\nFinal value: " + _state.finalValue;
            _state.history[_state.history.Count - 1].log += "\nThreshold: " + thr;
            _state.isFinished = true;
        }

        public void SetNextTrial(bool valueChange)
        {
            if (NextTrialIsACatch(valueChange))
            {
                SetCatchTrial();
            }
            else
            {
                _state.type = "test";
            }
        }

        bool NextTrialIsACatch(bool valueChange)
        {
            bool doCatch = false;

            if (startWithNcatch > 0 && _state.ncatchesDone < startWithNcatch)
            {
                _state.ncatchesDone++;
                doCatch = true;
            }
            else
            {
                doCatch = _state.type == "test";
                doCatch &= catches.Count > 0;
                doCatch &= (valueChange || !doAllCatches);
                doCatch &= UnityEngine.Random.Range(0f, 1f) <= pCatch;

                doCatch |= _state.catchValues.Count > 1;

                if (catchInterval == CatchInterval.Trial && !doAllCatches)
                {
                    doCatch = UnityEngine.Random.Range(0f, 1f) <= pCatch;
                }
            }

            return doCatch;
        }

        void SetCatchTrial()
        {
            _state.type = "catch";

            if (_state.catchValues.Count > 1)
            {
                // update an ongoing set of catch trials
                _state.catchValues.RemoveAt(0);
                return;
            }

            // evaluate catch trial expressions replacing the letter "A" with the current adapted value
            foreach (Variable v in catches)
            {
                KLib.Expressions.Evaluate(v.expression.Replace("A", _state.value.ToString()));
            }

            _state.type = "catch";
            _state.catchValues.Clear();

            if (!doAllCatches)
            {
                float[] v = new float[catches.Count];
                for (int k = 0; k < v.Length; k++)
                {
                    v[k] = catches[k].GetValue();
                }

                _state.catchValues.Add(v);
            }
            else // create new list of catch trial values
            {
                //bool isCatchTracked = catches[0].PropertyName == PropertyName;
                bool isCatchTracked = false;

                int[] iorder = KLib.KMath.Permute(catches[0].Length);
                foreach (int index in iorder)
                {
                    float[] v = new float[catches.Count];
                    for (int kv = 0; kv < v.Length; kv++)
                    {
                        int ix = index % catches[kv].Length;
                        v[kv] = catches[kv].GetValue(ix);
                    }

                    if (!isCatchTracked || (v[0] >= minVal && v[0] <= maxVal) || float.IsNegativeInfinity(v[0]))
                    {
                        _state.catchValues.Add(v);
                    }
                }
            }
        }


    }
}
