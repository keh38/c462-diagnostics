using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Cues
{ 
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class CueLayout
    {
        [Category("Design")]
        public string Name { get; set; }
        private bool ShouldSerializeName() { return false; }

        [Category("Layout")]
        [Description("Horizontal position of the center as fraction of the screen size")]
        public float X { get; set; }
        private bool ShouldSerializeX() { return false; }

        [Category("Layout")]
        [Description("Vertical position of the center as fraction of the screen size")]
        public float Y { get; set; }
        private bool ShouldSerializeY() { return false; }

       //public uint color = 0xFF000000;

        public CueLayout()
        {
            X = 0.5f;
            Y = 0.5f;
        }

    }
}
