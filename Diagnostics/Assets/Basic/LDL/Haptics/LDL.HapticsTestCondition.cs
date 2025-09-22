using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals;

namespace LDL.Haptics
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class HapticsTestCondition
    {
        public Laterality ear;
        public float Freq_Hz;
        public string hapticVariable;
        public float hapticValue;
        public bool offerBreakAfter = false;
        public List<float> discomfortLevel = new List<float>();

        public HapticsTestCondition() { }

        public HapticsTestCondition(Laterality ear, float Freq_Hz, string hapticVariable, float hapticValue)
        {
            this.ear = ear;
            this.Freq_Hz = Freq_Hz;
            this.hapticVariable = hapticVariable;
            this.hapticValue = hapticValue;
        }

    }
}