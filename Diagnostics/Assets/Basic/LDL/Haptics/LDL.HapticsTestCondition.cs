using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

using KLib.Signals;

namespace LDL.Haptics
{
    public class PropValPair
    {
        public string variable;
        public float value;
        public PropValPair() { }
    }

    public class PropValPairList : List<PropValPair> { }    

    [JsonObject(MemberSerialization.OptOut)]
    public class HapticsTestCondition
    {
        public Laterality ear;
        public float Freq_Hz;
        public List<PropValPair> propValPairs;
        public bool offerBreakAfter = false;
        public List<float> discomfortLevel = new List<float>();

        public HapticsTestCondition() { }

        public HapticsTestCondition(Laterality ear, float Freq_Hz)
        {
            this.ear = ear;
            this.Freq_Hz = Freq_Hz;
            this.propValPairs = new List<PropValPair>();
        }

    }
}