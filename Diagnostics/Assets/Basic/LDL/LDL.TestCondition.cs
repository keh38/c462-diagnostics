using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals;

namespace LDL
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class TestCondition
    {
        public Laterality ear;
        public float Freq_Hz;
        public bool offerBreakAfter = false;
        public List<float> discomfortLevel = new List<float>();

        public TestCondition()
        {
        }

        public TestCondition(Laterality ear, float Freq_Hz)
        {
            this.ear = ear;
            this.Freq_Hz = Freq_Hz;
        }

    }
}