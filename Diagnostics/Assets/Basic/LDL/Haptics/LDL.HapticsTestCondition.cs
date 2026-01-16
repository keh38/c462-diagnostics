using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

using KLib.Signals;
using System;

namespace LDL.Haptics
{
    [JsonObject(MemberSerialization.OptOut)]
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

    [JsonConverter(typeof(PropValPairListJsonConverter))]
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

        public void Set(string variable, float value)
        {
            foreach (PropValPair pair in this)
            {
                if (pair.variable == variable)
                {
                    pair.value = value;
                    return;
                }
            }
            this.Add(new PropValPair(variable, value));
        }

        public override string ToString()
        {
            string s = "";
            foreach (PropValPair pair in this)
            {
                s += pair.variable + "=" + pair.value + "; ";
            }
            return s;
        }
    }

    public class PropValPairListJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(PropValPairList))
                return true;
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (PropValPair pair in (PropValPairList)value)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("variable");
                writer.WriteValue(pair.variable);
                writer.WritePropertyName("value");
                writer.WriteValue(pair.value);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
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