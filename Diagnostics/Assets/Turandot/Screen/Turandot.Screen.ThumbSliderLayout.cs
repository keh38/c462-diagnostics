using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class ThumbSliderLayout
    {
        public int sliderY = 250;
        public int thumbX = 800;
        public int thumbY = -475;

        public ThumbSliderLayout() { }
    }
}
