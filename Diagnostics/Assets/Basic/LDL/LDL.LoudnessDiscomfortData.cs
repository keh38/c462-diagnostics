using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace LDL
{
    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class LoudnessDiscomfortData
    {
        public Audiograms.AudiogramData LDLgram = null;
        public List<TestCondition> testConditions;
        public List<SliderSettings> sliderSettings = new List<SliderSettings>();
    }
}