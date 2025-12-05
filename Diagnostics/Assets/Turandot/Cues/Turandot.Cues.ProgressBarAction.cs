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
    public class ProgressBarAction : Cue
    {
        public ProgressBarAction() { }

        [XmlIgnore]
        [ProtoIgnore]
        [JsonIgnore]
        override public string Name
        {
            get { return "Progress bar"; }
        }

    }
}
