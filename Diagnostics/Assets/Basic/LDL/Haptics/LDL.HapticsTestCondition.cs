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
        public PropValPair(string variable, float value)
        {
            this.variable = variable;
            this.value = value;
        }
    }

    public class PropValPairList : List<PropValPair>
    {
        public PropValPairList Clone()
        {
            var clone = new PropValPairList();
            foreach (PropValPair pair in this)
            {
                clone.Add(new PropValPair(pair.variable, pair.value));
            }
            return clone;
        }
    }    

    [JsonObject(MemberSerialization.OptOut)]
    public class HapticsTestCondition
    {
        public Laterality ear;
        public float Freq_Hz;
        public PropValPairList propValPairs;
        public bool offerBreakAfter = false;
        public List<float> discomfortLevel = new List<float>();

        public HapticsTestCondition() { }

        public HapticsTestCondition(Laterality ear, float Freq_Hz)
        {
            this.ear = ear;
            this.Freq_Hz = Freq_Hz;
            this.propValPairs = new PropValPairList();
        }

    }
}