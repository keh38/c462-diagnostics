using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace LDL
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class SliderSettings
    {
        public KLib.Signals.Laterality ear;
        public float Freq_Hz;
        public string var;
        public float min;
        public float max;
        public float start;
        public float end;
        public bool isMaxed;
    }
}