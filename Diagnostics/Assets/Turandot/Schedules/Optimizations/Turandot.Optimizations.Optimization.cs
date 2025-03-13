using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;


using Newtonsoft.Json;
using ProtoBuf;

using Turandot.Schedules;

namespace Turandot.Optimizations
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    [XmlInclude(typeof(AnnealAFC))]
    [XmlInclude(typeof(AnnealContin))]
    public class Optimization
    {
        public string storeAs = "";
        public Metric metric = Metric.mAFC;
        public List<Variable> variables = new List<Variable>();
        public List<Variable> nonoptVar = new List<Variable>();

        protected int _maxNumberOfTrials = -1;
        protected bool _isFinished = false;

        protected string _log;

        [JsonIgnore]
        [XmlIgnore]
        public int MaxNumberOfTrials { get { return _maxNumberOfTrials; } }

        [JsonIgnore]
        [XmlIgnore]
        public bool IsFinished { get { return _isFinished; } }

        [JsonIgnore]
        [XmlIgnore]
        public string Log { get { return _log; } }

        protected List<string> _synonymsForCorrect = new List<string>(new string[] { "go", "right", "correct" });

        public virtual void Initialize()
        {
            _isFinished = false;
            _log = "";
        }
        public virtual SCLElement InitTrial() { return null; }
        public virtual void ProcessResult(string result) { }
        public virtual string ToJSONString() { return ""; }
        public virtual List<Parameter> Best() { return null; }

        protected void AddLog(string text)
        {
            Debug.Log(text);
            _log += text + System.Environment.NewLine;
        }

#if KDEBUG
        public virtual string Simulate() { return "go"; }
#endif

    }
}