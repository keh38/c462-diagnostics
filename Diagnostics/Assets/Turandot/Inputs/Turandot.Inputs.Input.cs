using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using ProtoBuf;
using Newtonsoft.Json;

namespace Turandot.Inputs
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    [XmlInclude(typeof(Button))]
    [XmlInclude(typeof(Categorizer))]
    [XmlInclude(typeof(GrapherAction))]
    [XmlInclude(typeof(Keypad))]
    [XmlInclude(typeof(ParamSliderAction))]
    [XmlInclude(typeof(RandomProcess))]
    [XmlInclude(typeof(SAM))]
    [XmlInclude(typeof(Scaler))]
    [XmlInclude(typeof(ThumbSliderAction))]
    public class Input
    {
        public string label = "";
        [Category("Action")]
        public bool BeginVisible { get; set; }
        private bool ShouldSerializeBeginVisible() { return false; }

        [Category("Action")]
        public bool EndVisible { get; set; }
        private bool ShouldSerializeEndVisible() { return false; }

        public bool enabled = true;

        public int X = 0;
        public int Y = 0;
        public float value;
        public string name;

        [ReadOnly(true)]
        public string Target { get; set; }

        public Input()
        {
            BeginVisible = true;
            EndVisible = false;
        }
        public Input(string name) : this()
        {
            this.name = name;
        }

        [XmlIgnore]
        [ProtoIgnore]
        [JsonIgnore]
        virtual public string Name
        {
            get { return name; }
        }
    }
}
