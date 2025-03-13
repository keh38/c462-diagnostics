using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class MessageLayout
    {
        public int fontSize = 72;
        public uint color = 0xFF000000;
        public int X = 0;
        public int Y = 0;

        public MessageLayout() { }
    }
}
