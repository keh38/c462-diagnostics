using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using KLib;
using KLib.Signals.Enumerations;

using Turandot;
using Turandot.Schedules;

namespace Turandot.Scripts
{
    public class TurandotScripter : MonoBehaviour
    {            // TURANDOT FIX

        /*
        internal class SeqElement
        {
            public string file = "";
            public float value = float.NaN;
            public int group = -1;
            public bool instruct = false;
            public Laterality ear = Laterality.Unspecified;
            public float[] values;
            public int[] groups;

            private bool _isVector = false;

            public SeqElement() { }
            public SeqElement(string file, Laterality ear) { this.file = file; this.ear = ear; }
            public SeqElement(string file, Laterality ear, float value, int group, bool instruct)
            {
                this.file = file;
                this.ear = ear;
                this.value = value;
                this.group = group;
                this.instruct = instruct;
                _isVector = false;
                Debug.Log(ToString());
            }
            public SeqElement(string file, Laterality ear, float[] values, int[] groups, bool instruct)
            {
                this.file = file;
                this.ear = ear;
                this.values = values;
                this.groups = groups;
                this.instruct = instruct;
                _isVector = true;
                Debug.Log(ToString());
            }
            public override string ToString()
            {
                if (_isVector)
                    return file + ", values: " + values.Length + ", groups: " + groups.Length + ", instruct: " + instruct;

                return file + ", value: " + value + ", group: " + group + ", instruct: " + instruct;

            }
            public bool IsVector { get { return _isVector; } }
        }

        private int _seqIndex;
        private Script _script;

        private bool _isFinished = false;
        private string _paramFile;

        private List<SeqElement> _sequence = new List<SeqElement>();

        // Make singleton
        private static TurandotScripter instance;
        public static TurandotScripter Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject gobj = GameObject.Find("TurandotScripter");
                    if (gobj != null)
                    {
                        instance = gobj.GetComponent<TurandotScripter>();
                    }
                    else
                    {
                        instance = new GameObject("TurandotScripter").AddComponent<TurandotScripter>();
                    }
                    instance.FinishInit();
                }
                return instance;
            }
        }

        private void FinishInit()
        {
            DontDestroyOnLoad(this);
        }

        public bool IsFinished
        {
            get { return _isFinished; }
        }

        public string ParamFile
        {
            get { return _paramFile; }
        }

        public string LinkTo
        {
            get { return _script.linkTo; }
        }

        public void Initialize(string scriptPath)
        {
            _script = FileIO.XmlDeserialize<Script>(scriptPath);
            _seqIndex = 0;
            _isFinished = false;

            float[] values = (string.IsNullOrEmpty(_script.Values)) ? null : Expressions.Evaluate(_script.Values);

            if (_script.SplitAfter > 0)
            {
                values = SplitScript(scriptPath, values);
            }

            int[] groups = (string.IsNullOrEmpty(_script.Groups)) ? null : Expressions.EvaluateToInt(_script.Groups);

            if (values != null)
            {
                float[] tmpvals = new float[values.Length];
                int[] tmpgrps = new int[values.Length];

                int idx = 0;
                foreach (int i in KMath.Permute(values.Length))
                {
                    tmpvals[idx] = values[i];
                    tmpgrps[idx] = (groups == null) ? -1 : groups[i % groups.Length];
                    idx++;
                }

                values = tmpvals;
                groups = tmpgrps;
            }

            if (_script.Repeats > 1)
            {
                List<string> configRepeats = new List<string>();
                if (_script.order == Order.Sequential)
                {
                    for (int k = 0; k < _script.Repeats; k++)
                    {
                        foreach (string f in _script.ConfigFiles) configRepeats.Add(f);
                    }
                }
                else if (_script.order == Order.Interleave)
                {
                    foreach (int i in KMath.Permute(_script.ConfigFiles.Count * _script.Repeats))
                    {
                        configRepeats.Add(_script.ConfigFiles[i % _script.ConfigFiles.Count]);
                    }
                }
                _script.ConfigFiles = configRepeats;
            }

            _sequence.Clear();
            if (values == null)
            {
                foreach (Laterality ear in GetTestEars(_script.TestEars))
                {
                    foreach (string f in _script.ConfigFiles) _sequence.Add(new SeqElement(f, ear));
                }
            }
            else
            {
                if (_script.oneRun)
                {
                    List<Laterality> ears = GetTestEars(_script.TestEars);
                    for (int ke = 0; ke < ears.Count; ke++)
                    {
                        for (int kc = 0; kc < _script.ConfigFiles.Count; kc++)
                        {
                            _sequence.Add(new SeqElement(_script.ConfigFiles[kc], ears[ke], values, groups, ke == 0 && kc == 0));
                        }
                    }
                }
                else
                {
                    for (int k = 0; k < values.Length; k++)
                    {
                        List<Laterality> ears = GetTestEars(_script.TestEars);
                        for (int ke = 0; ke < ears.Count; ke++)
                        {
                            for (int kc = 0; kc < _script.ConfigFiles.Count; kc++)
                            {
                                _sequence.Add(new SeqElement(_script.ConfigFiles[kc], ears[ke], values[k], groups[k], k > 0 && ke == 0 && kc == 0));
                            }
                        }
                    }
                }
            }
        }

        private float[] SplitScript(string path, float[] values)
        {
            int nfile = Mathf.CeilToInt(values.Length / _script.SplitAfter);

            var v = KLib.KMath.Permute(values);

            int nperFile = _script.SplitAfter;
            _script.SplitAfter = 0;

            int i1 = nperFile;
            for (int k=1; k<nfile; k++)
            {
                _script.Values = "[";
                int i2 = Mathf.Min(i1 + nperFile, v.Length);
                for (int kv = i1; kv < i2; kv++) _script.Values += v[kv].ToString() + " "; 
                _script.Values += "]";
                Debug.Log("Split " + k + ": " + _script.Values);

                KLib.FileIO.XmlSerialize(_script, path.Replace(".xml", "_" + k + ".xml"));

                i1 = i2;
            }


            float[] theseValues = new float[nperFile];
            for (int k = 0; k < nperFile; k++) theseValues[k] = v[k];

            return theseValues;
        }


        public Parameters Next()
        {
            SeqElement seqElem = _sequence[_seqIndex];

            _paramFile = DataFileLocations.ConfigFile("Turandot", seqElem.file);
            Parameters p = FileIO.XmlDeserialize<Parameters>(_paramFile);

            if (!float.IsNaN(seqElem.value) || seqElem.IsVector)
            {
                if (p.schedule.mode == Mode.Adapt)
                {
                    foreach (AdaptiveTrack t in p.adapt.tracks)
                    {
                        Variable v = t.variables.Find(o => o.dim == _script.Dim);
                        if (v != null) v.expression = seqElem.value.ToString();
                    }
                }
                else
                {
                    if (seqElem.group >= 0)
                    {
                        Family keep = p.schedule.families[seqElem.group];
                        p.schedule.families.Clear();
                        p.schedule.families.Add(keep);
                    }

                    foreach (Family f in p.schedule.families)
                    {
                        Variable v = f.variables.Find(o => o.dim == _script.Dim);
                        if (v != null)
                        {
                            if (seqElem.IsVector)
                            {
                                string vec = "[";
                                foreach(var value in seqElem.values) vec += value.ToString() + " ";
                                vec += "]";
                                v.expression = vec;
                                Debug.Log(vec);
                            }
                            else
                                v.expression = seqElem.value.ToString();
                        }
                    }
                }

                Debug.Log("Running " + System.IO.Path.GetFileName(_paramFile) + " @ " + seqElem.value);
            }
            else
            {
                Debug.Log("Running " + System.IO.Path.GetFileName(_paramFile));
            }

            if (seqElem.instruct && _script.SecondaryInstructions != null)
            {
                p.instructions = _script.SecondaryInstructions;
            }


            if (seqElem.ear != Laterality.Unspecified)
            {
                foreach (FlowElement fe in p.flowChart)
                {
                    if (fe.sigMan != null)
                    {
                        foreach (KLib.Signals.Channel ch in fe.sigMan.channels) ch.Destination = seqElem.ear;
                    }
                }

                foreach (var f in p.schedule.families)
                {
                    foreach (var v in f.variables)
                    {
                        v.expression = LateralizeExpression(v.expression, "THR", seqElem.ear);
                        v.expression = LateralizeExpression(v.expression, "LDL", seqElem.ear);
                    }
                }
            }

            return p;
        }


        private string LateralizeExpression(string expression, string metric, Laterality ear)
        {
            string pattern = @"(" + metric + @"\(([LlRrBbDd])\s*,)";
            Match m = Regex.Match(expression, pattern);

            while (m.Success)
            {
                Debug.Log(m.Groups[0].Value);
                expression = expression.Replace(m.Groups[0].Value, metric + "(" + ear.ToString().Substring(0, 1) + ",");
                m = m.NextMatch();
            }

            return expression;
        }

        public void Advance()
        {
            if (++_seqIndex == _sequence.Count)
            {
                _isFinished = true;
            }
        }

        private List<Laterality> GetTestEars(string spec)
        {
            List<Laterality> ears = new List<Laterality>();
            List<string> names = new List<string>(Enum.GetNames(typeof(Laterality)));
            if (!string.IsNullOrEmpty(spec))
            {
                foreach (string e in spec.Split(','))
                {
                    int idx = names.FindIndex(o => o == e.Trim());
                    if (idx >= 0) ears.Add((Laterality)idx);
                }

                List<Laterality> tmp = new List<Laterality>();
                foreach (int k in KMath.Permute(ears.Count)) tmp.Add(ears[k]);
                ears = tmp;
            }
            else
            {
                ears.Add(Laterality.Unspecified);
            }

            return ears;
        }
        */

    }
}
 