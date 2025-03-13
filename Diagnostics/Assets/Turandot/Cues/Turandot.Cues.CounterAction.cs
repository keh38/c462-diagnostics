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
    public class CounterAction : Cue
    {
        public enum Action { None, Reset, Start, Stop}

        public Action action;
        public int maxValue = 0;
        public int minValue = 0;
        public float delay_s = 0;
        public float interval_s = 1f;
        public int incrementBy = 1;
        public int startAt = 10;
        public bool changeColor = false;
        public uint newColor = 0xFF000000;

        public CounterAction()
        {
        }

        [XmlIgnore]
        [ProtoIgnore]
        [JsonIgnore]
        override public string Name
        {
            get { return "Counter"; }
        }

    }
}
