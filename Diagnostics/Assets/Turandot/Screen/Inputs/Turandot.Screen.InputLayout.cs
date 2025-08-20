using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

        public InputLayout()
        {
            X = 0.5f;
            Y = 0.5f;
        }

    }
}
