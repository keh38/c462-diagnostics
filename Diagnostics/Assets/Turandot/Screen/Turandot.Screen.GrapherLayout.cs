using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class GrapherLayout
    {
        public float speed = 0.1f;
        public float graphX = 0;
        public float graphY = 0;
        public float graphW = 3.6f;
        public float graphH = 1.0f;
        public float stylusX = -1.75f;
        public float stylusW = 0.025f;
        public uint inkColor = 0xFFFF0000;
        public bool mustContactStylus = true;
        public bool stylusPositionFixed = false;

        public GrapherLayout() { }
    }
}
