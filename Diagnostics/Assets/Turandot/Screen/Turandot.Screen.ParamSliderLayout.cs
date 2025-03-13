using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class ParamSliderLayout
    {
        public int X = 0;
        public int Y = 0;
        public int width = 1250;
        public int height = 50;

        public ParamSliderLayout() { }
    }
}
