using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Termination
    {
        public TermType type = TermType.Any;
        public string source;
        public string linkTo = "";
        public float latency_ms = 0;
        public string result = "";
        public string flagExpr = "";
        public TerminationAction action = TerminationAction.EndImmediately;

        public Termination()
        {
        }

        public Termination(string source)
        {
            this.source = source;
        }
        public Termination(string source, string linkTo)
        {
            this.source = source;
            this.linkTo = linkTo;
        }
        public Termination(string source, string linkTo, TermType termType)
        {
            this.source = source;
            this.linkTo = linkTo;
            this.type = termType;
            switch (termType)
            {
                case TermType.CSplus:
                    this.result = "Hit";
                    break;
                case TermType.CSminus:
                    this.result = "False Alarm";
                    break;
                default:
                    this.result = "";
                    break;
            }
        }
        public Termination(string source, string linkTo, TerminationAction action)
        {
            this.source = source;
            this.linkTo = linkTo;
            this.action = action;
        }
        public Termination(string source, TerminationAction action)
        {
            this.source = source;
            this.action = action;
        }

        public void Initialize()
        {
        }

    }
}
