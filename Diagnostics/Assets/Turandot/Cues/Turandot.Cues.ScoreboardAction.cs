using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Cues
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class ScoreboardAction : Cue
    {
        public enum ScoreAction { None, Change}

        public ScoreAction action;
        public string deltaExpr = "";
        public int pointsPerSec = 0;
        public float maxSec = 5f;

        public ScoreboardAction()
        {
        }

        [XmlIgnore]
        [ProtoIgnore]
        [JsonIgnore]
        override public string Name
        {
            get { return "Scoreboard"; }
        }

    }
}
