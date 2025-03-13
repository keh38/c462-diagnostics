using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

using ExtensionMethods;

namespace Turandot
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Timeout
    {
        public string expr = "1";
        public string linkTo = "";
        public TermType termType = TermType.Any;
        public string result = "";

        float _value;
        bool _sequenced = false;

        public Timeout()
        { }

        public Timeout(string expr, string linkTo)
        {
            this.expr = expr;
            this.linkTo = linkTo;
        }

        public Timeout(string expr, string linkTo, TermType termType)
        {
            this.expr = expr;
            this.linkTo = linkTo;
            this.termType = termType;
            this.result = termType == TermType.CSplus ? "Miss" : "Withhold";
        }

        public void Initialize()
        {
            _sequenced = false;
            _value = KLib.Expressions.Evaluate(expr).GetRandom();
        }

        [XmlIgnore]
        [JsonIgnore]
        public float Value
        {
            get
            {
                if (!_sequenced)
                {
                    _value = KLib.Expressions.Evaluate(expr).GetRandom();
                }

                return _value;
            }
            set
            {
                _value = value;
                _sequenced = true;
            }
        }

    }
}
