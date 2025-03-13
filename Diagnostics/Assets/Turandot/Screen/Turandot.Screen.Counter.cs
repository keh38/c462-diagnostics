using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Counter
    {
        public int X = 0;
        public int Y = 300;
        public int size = 100;
        public uint color = 0xFFFFFFFF;

        public Counter() { }
    }
}
