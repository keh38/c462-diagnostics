using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

using C462.Shared;

namespace CombinedAudioLDL
{
    [JsonObject(MemberSerialization.OptOut)]
    public class TestCondition
    {
        public Laterality ear;
        public float Freq_Hz;
        public bool offerBreakAfter = false;
        public float threshold;
        public float LDL;

        public TestCondition() { }

        public TestCondition(Laterality ear, float Freq_Hz)
        {
            this.ear = ear;
            this.Freq_Hz = Freq_Hz;
        }

    }
}