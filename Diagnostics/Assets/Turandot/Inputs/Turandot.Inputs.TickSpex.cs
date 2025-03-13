using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Inputs
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class TickSpex
    {
        public enum TickType { None, Linear, Arbitrary};

        public TickType tickType = TickType.None;
        public float min = 0;
        public float max = 10;
        public float step = 1;
        public List<string> labels = null;

        public TickSpex()
        {
        }
    }
}
