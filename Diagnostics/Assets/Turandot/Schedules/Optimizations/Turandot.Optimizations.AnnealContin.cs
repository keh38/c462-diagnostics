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
    public class AnnealContin : Optimization
    {
        internal class AnnealVar
        {
            public string name;
            public float min;
            public float max;
            public float range;
            public float center;
            public float best;
            public List<float> values;
            
            //private GaussianRandom _rng;

            //public AnnealVar(string name, float min, float max, float temp, GaussianRandom rng)
            //{
            //    var s = name.Split('.');
            //    if (s.Length > 1) name = s[1];

            //    //_rng = rng;

            //    this.name = name;
            //    this.min = min;
            //    this.max = max;
            //    range = (max - min);
            //    center = (max + min) / 2f;

            //    values = new List<float>();

            //    best = InitSolution();
            //}

            public float InitSolution()
            {
                float initVal = min + UnityEngine.Random.Range(0f, 1f) * range;
                values.Add(initVal);
                return initVal;
            }

            public void SetBest(float v)
            {
                values[values.Count - 1] = v;
                best = v;
            }

            public float NewSolution(float temp)
            {
                float newVal = float.NegativeInfinity;
                float delta = temp/4f * range;
                while (newVal < min || newVal > max)
                {
                    //newVal = best + delta * _rng.Next();
                }

                values.Add(newVal);
                return newVal;
            }

            public float LastValue()
            {
                return values[values.Count - 1];
            }

            public void SwitchOverallBest()
            {
                best = LastValue();
            }
        }

        public int maxTries = 10;
        public int maxSuccess = 5;
        public int maxConsRej = 12;
        public int maxIteration = 70;
        public int numInitTrials = 20;
        public float initTemp = 1;
        public float stopTemp = 1e-8f;
        public float coolRate = 0.8f;
        public float stopVal = float.NegativeInfinity;
        public string startAt = "";

        private float _initEnergy;
        private float _oldEnergy;

        private int _itry = 0;
        private int _success = 0;
        private int _consec = 0;
        private int _count = 0;
        private float _temp;
        private float _boltzmann = 1f;
        private List<AnnealVar> _vars = new List<AnnealVar>();
        private string _terminationCause;
        private bool _completedSuccessfully;

        //private GaussianRandom _rng = new GaussianRandom();

        public override void Initialize()
        {
            _itry = 0;
            _success = 0;
            _consec = 0;
            _count = 0;
            _temp = initTemp;
            _terminationCause = "";
            _completedSuccessfully = false;

            foreach (var v in variables)
            {
                float[] minmax = Expressions.Evaluate(v.expression);
                //_vars.Add(new AnnealVar(v.property, KMath.Min(minmax), KMath.Max(minmax), _temp, _rng));
            }

            if (!string.IsNullOrEmpty(startAt))
            {
                foreach (var v in _vars)
                {
                    float value = KLib.Expressions.EvaluateToFloatScalar("M(" + startAt + "." + v.name + ")");
                    v.SetBest(value);
                }
            }

            _maxNumberOfTrials = maxIteration;

            base.Initialize();
        }

        public override SCLElement InitTrial()
        {
            SCLElement sc = new SCLElement(0, 0);
            string state = variables[0].state;
            string chan = variables[0].chan;

            for (int k = 0; k < variables.Count; k++)
            {
                sc.propValPairs.Add(new PropValPair(variables[k].PropertyName, _vars[k].LastValue()));
            }

            foreach (var pvp in sc.propValPairs) AddLog(pvp.ToString()); ;

            return sc;
        }

        internal class Trace
        {
            public float[] t;
            public float[] y;
        }

        public override void ProcessResult(string response)
        {
            var s = response.Split('=');

            Trace trace = KLib.FileIO.JSONDeserializeFromString<Trace>(s[1]);
            float ss = 0;
            for (int k = 0; k < trace.y.Length; k++) ss += trace.y[k] * trace.y[k];

            float newEnergy = 1.0f / ss;
            //float newEnergy = trace.y.Length - ss;
            AddLog("-> Energy = " + newEnergy);

            _count++;

            if (_count >= maxIteration)
            {
                _isFinished = true;
                FinishLog("Max iterations exceeded");
                return;
            }

            if (_count == 1)
            {
                _initEnergy = newEnergy;
                _oldEnergy = newEnergy;
            }
            else
            {
                if (newEnergy < stopVal)
                {
                    foreach (var v in _vars) v.SwitchOverallBest();
                    _oldEnergy = newEnergy;
                    _isFinished = true;
                    _completedSuccessfully = true;
                    FinishLog("Minimum energy reached");
                    return;
                }

                if (_oldEnergy - newEnergy > 1e-6f)
                {
                    foreach (var v in _vars) v.SwitchOverallBest();
                    _oldEnergy = newEnergy;
                    _success++;
                    _consec = 0;
                    AddLog("-->Success type 1");
                    foreach (var v in _vars) AddLog(v.name + "=" + v.best);
                }
                else
                {
                    if (UnityEngine.Random.Range(0f, 1f) < Mathf.Exp((_oldEnergy-newEnergy)/(_boltzmann * _temp)))
                    {
                        foreach (var v in _vars) v.SwitchOverallBest();
                        _oldEnergy = newEnergy;
                        _success++;
                        AddLog("-->Success type 2");
                        foreach (var v in _vars) AddLog(v.name + "=" + v.best);
                    }
                    else
                    {
                        _consec++;
                    }
                }
            }

            _itry++;

            if (_count > numInitTrials && (_itry >= maxTries || _success >= maxSuccess))
            {
                if (_temp < stopTemp || _consec >= maxConsRej)
                {
                    _isFinished = true;
                    _completedSuccessfully = true;
                    if (_temp < stopTemp) FinishLog("Cooling completed");
                    else if (_consec >= maxConsRej) FinishLog("Exceeded max number of consecutive rejections");
                    return;
                }
                else
                {
                    _temp *= coolRate;
                    _itry = 1;
                    _success = 1;
                    AddLog("--> Lower temperature: " + _temp);
                }
            }

            if (_count > numInitTrials)
            {
                foreach (var v in _vars) v.NewSolution(_temp);
            }
            else
            {
                foreach (var v in _vars) v.InitSolution();
            }
        }

        private void FinishLog(string message)
        {
            _terminationCause = message;
            AddLog("--> " + message);
            AddLog(" ");
            AddLog("Success: " + _completedSuccessfully.ToString().ToUpper());
            AddLog("Initial temperature: " + initTemp);
            AddLog("Final temperature: " + _temp);
            AddLog("Consecutive rejections: " + _consec);
            AddLog("Number of evaluations: " + _count);
            AddLog("Total final energy: " + _oldEnergy);

            AddLog(" ");
            foreach (var v in _vars) AddLog(storeAs + "." + v.name + " = " + v.best);
        }

        public override string ToJSONString()
        {
            string json = "{\"" + storeAs + "\":{";

            for (int k=0; k<_vars.Count; k++)
            {
                json += "\"" + _vars[k].name + "\":" + _vars[k].best.ToString() + (k<_vars.Count-1 ? "," : "");
            }
            json += "}}";
            return json;
        }

        public override List<Parameter> Best()
        {
            List<Parameter> best = new List<Parameter>();
            foreach (var v in _vars) best.Add(new Parameter(v.name, v.best));

            return best;
        }

#if KDEBUG
        public override string Simulate()
        {
            float[] target = new float[] { 3350, 0.8f };

            float strength = 1;
            for (int k=0; k<_vars.Count; k++)
            {
                float delta = 5f * (target[k] - _vars[k].LastValue()) / _vars[k].range;
                strength *= Mathf.Exp(-delta * delta);
            }

            float dt = 0.025f;
            float Tmax = 3f;
            float tau = 0.35f;
            int npts = Mathf.RoundToInt(Tmax / dt);
            float[] t = new float[npts];
            float[] y = new float[npts];

            for (int k=0; k< npts; k++)
            {
                t[k] = k * dt;
                y[k] = strength * Mathf.Exp(-t[k] / tau);
            }

            string tstr = "";
            string ystr = "";

            for (int k = 0; k < npts; k++)
            {
                tstr += t[k].ToString("F3") + ",";
                ystr += y[k].ToString("F3") + ",";
            }

            return "trace={\"t\":[" + tstr + "],\"y\":[" + ystr + "]}";
        }

#endif
    }
}