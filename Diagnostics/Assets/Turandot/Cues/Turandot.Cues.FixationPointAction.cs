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
    public class FixationPointAction : Cue
    {
        public float angle = 0f;
        public string label = "";

        public FixationPointAction() { }

        [XmlIgnore]
        [ProtoIgnore]
        [JsonIgnore]
        override public string Name
        {
            get { return "Fixation point"; }
        }

    }
}
