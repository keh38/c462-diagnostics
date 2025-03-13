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
    public class Button : Input
    {
        public float delay_ms = 0f;
        public float duration_ms = 200f;
        public float interval_ms = 500f;
        public int numFlash = 0;
        public bool tweenScale = false;
        public float scaleTo = 2;
        public bool changeAppearance = false;
        public uint color = 0xFFFFFF;

        public Button() : base("Button") { }
        public Button(string name) : base(name) { }
    }
}
