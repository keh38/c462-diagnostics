using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class ProgressBarLayout
    {
        public int X =-450;
        public int Y = 450;
        public int W = 900;
        public int H = 33;

        public ProgressBarLayout() { }
    }
}
