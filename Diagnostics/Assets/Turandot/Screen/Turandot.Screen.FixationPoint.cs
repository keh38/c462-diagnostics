using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class FixationPoint
    {
        public enum Style { Default, Ear, Bar}

        public Style style = Style.Default;
        public int size = 100;
        public uint color = 0xFF000000;
        public uint barColor = 0xFFFFFFFF;
        public int barWidth = 25;
        public float barAngle = 0;
        public string label = "";

        public FixationPoint() { }
    }
}
