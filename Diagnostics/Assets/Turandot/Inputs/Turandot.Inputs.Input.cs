using System;
using System.Collections.Generic;
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
        public bool startVisible = true;
        public bool endVisible = false;
        public bool enabled = true;
        public int X = 0;
        public int Y = 0;
        public float value;
        public string name;

        public Input() { }
        public Input(string name)
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
