using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [XmlInclude(typeof(ButtonLayout))]
    [XmlInclude(typeof(ChecklistLayout))]
    [XmlInclude(typeof(ManikinLayout))]
    [XmlInclude(typeof(ParamSliderLayout))]
    [XmlInclude(typeof(ScalerLayout))]
    [JsonObject(MemberSerialization.OptOut)]
    public class InputLayout
    {
        public string Name { get; set; }
        private bool ShouldSerializeName() { return false; }

        public float X { get; set; }
        private bool ShouldSerializeX() { return false; }

        public float Y { get; set; }
        private bool ShouldSerializeY() { return false; }

        public InputLayout()
        {
            X = 0.5f;
            Y = 0.5f;
        }

    }
}
