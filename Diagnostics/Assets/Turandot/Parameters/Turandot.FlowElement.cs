using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals;

using Turandot.Schedules;

namespace Turandot
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class FlowElement
    {
        public string name = "";
        public bool isAction = false;
        public SignalManager sigMan = null;
        public List<Cues.Cue> cues = new List<Cues.Cue>();
        public List<Inputs.Input> inputs = new List<Inputs.Input>();
        public List<Timeout> timeOuts = new List<Timeout>();
        public List<Termination> term = new List<Termination>();
        public string ipcCommand = "";
        public EndAction endAction = EndAction.None;
        public string actionFamily = "";
        public bool hideCursor = false;

        private float _delay;
        private float _isi;

        private StimConList _scl = null;
        private int _sclIndex = 0;
        private string _sclLog = "";

        public FlowElement()
        {
        }

        public FlowElement(string name)
        {
            this.name = name;
            timeOuts.Add(new Timeout());
        }

        public void Initialize()
        {
            foreach (Timeout to in timeOuts)
            {
                to.Initialize();
            }
        }

        public Termination AddTermination(Termination t)
        {
            term.Add(t);
            return t;
        }

        public Timeout GetTimeout(Schedules.TrialType trialType)
        {
            Timeout timeOut = null;

            if (trialType == Schedules.TrialType.CSplus)
                timeOut = timeOuts.Find(to => to.termType == TermType.CSplus);
            else if (trialType == Schedules.TrialType.CSminus)
                timeOut = timeOuts.Find(to => to.termType == TermType.CSminus);

            return timeOut ?? timeOuts[0];
        }

        public void SetISI()
        {
            if (sigMan["Test"] != null)
            {
                _delay = sigMan["Test"].gate.Delay_ms;
                _isi = sigMan["Ref1"].gate.Delay_ms - _delay;
            }
        }

        public string SetParameter(string property, float value)
        {
            string error = "";

            string[] s = property.Split(new char[] { '.' }, 2);
            switch (s[0])
            {
                case "Timeout":
                    foreach (Timeout to in timeOuts)
                    {
                        to.Value = value;
                    }
                    break;

                default:
                    if (s.Length == 2)
                    {
                        error = sigMan.SetParameter(s[0], s[1], value);
                    }
                    else
                    {
                        error = name + ": unknown property '" + property + "'";
                    }
                    break;
            }

            return error;
        }

        public string SetCueProperty(string property, float value)
        {
            string result = "";

            var parts = property.Split(new char[] { '.' });

            if (parts.Length < 2) return "";

            var cue = cues.Find(x => x.Name == parts[0]);
            if (cue != null)
            {
                result = cue.SetProperty(parts[1], value);
            }

            return result;
        }

        public bool HasSequence
        {
            get { return isAction && !string.IsNullOrEmpty(actionFamily) && _scl != null && _scl.Count > 0; }
        }

        public void InitializeSCL(Family fam, int numBlocks)
        {
            var keepVar = fam.variables.FindAll(x => x.state.Equals(name));
            if (keepVar == null)
                throw new System.Exception("Family '" + fam.name + "' does not apply to action '" + name + "'");

            fam.variables = keepVar;

            foreach (var v in fam.variables) v.state = "";

            _scl = fam.CreateStimConList(Schedules.Mode.Sequence, numBlocks, false);

            _sclIndex = 0;
            _sclLog = "";
        }

        public bool AdvanceSequence()
        {
            if (_sclIndex >= _scl.Count) return false;

            foreach (var pv in _scl[_sclIndex].propValPairs)
            {
                //Debug.Log("Action(" + name + "): " + pv.property + "=" + pv.value);
                var error = SetParameter(pv.property, pv.value);

                if (!string.IsNullOrEmpty(error))
                    throw new System.Exception(error);
            }

            if (!string.IsNullOrEmpty(_sclLog))
            {
                _sclLog += "," + System.Environment.NewLine;
            }

            string propJSON = "";
            foreach (var b in CreatePropertyTree(_scl[_sclIndex].propValPairs))
            {
                propJSON = b.AddToJSON(propJSON, true).Substring(1);
            }
            Debug.Log(propJSON);

            _sclLog += "{" + propJSON + "}";
            _sclIndex++;

            return true;
        }

        public string ActionSequenceJSONString
        {
            get
            {
                return "[" + System.Environment.NewLine + _sclLog + System.Environment.NewLine + "]" + System.Environment.NewLine;
            }
        }

        List<TrialData.PropertyBranch> CreatePropertyTree(List<PropValPair> propVals)
        {
            var tree = new TrialData.PropertyBranch("root");

            foreach (var pv in propVals) tree.Add(pv.property + "=" + pv.value);

            return tree.children;
        }


    }
}
