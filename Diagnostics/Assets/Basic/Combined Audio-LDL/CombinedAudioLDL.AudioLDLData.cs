using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace CombinedAudioLDL
{
    [JsonObject(MemberSerialization.OptOut)]
    public class AudioLDLData
    {
        public Audiograms.AudiogramData audiogram = null;
        public Audiograms.AudiogramData LDLgram = null;
        public List<TestCondition> testConditions;
    }
}