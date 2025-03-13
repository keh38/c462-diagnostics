using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Scoreboard
    {
        public int X = 900;
        public int Y = 525;
        public int size = 100;
        public uint color = 0xFF0CF00C;
        public uint negativeColor = 0xFFF00C0C;

        public Scoreboard() { }
    }
}
