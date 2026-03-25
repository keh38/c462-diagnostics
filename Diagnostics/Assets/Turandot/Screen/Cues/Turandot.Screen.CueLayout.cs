using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Screen
{ 
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    [XmlInclude(typeof(FixationPointLayout))]
    [XmlInclude(typeof(ImageLayout))]
    [XmlInclude(typeof(MessageLayout))]
    [XmlInclude(typeof(ProgressBarLayout))]
    [XmlInclude(typeof(TextBoxLayout))]
    [XmlInclude(typeof(VideoLayout))]
    public class CueLayout
    {
        public string Name { get; set; }
        private bool ShouldSerializeName() { return false; }

        public float X { get; set; }
        private bool ShouldSerializeX() { return false; }

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
