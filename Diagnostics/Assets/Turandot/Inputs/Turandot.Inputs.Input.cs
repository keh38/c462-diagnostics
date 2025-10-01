using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Turandot.Inputs
{
    public enum EnabledState { Enabled, Disabled, Grayed}

    [JsonObject(MemberSerialization.OptOut)]
    [XmlInclude(typeof(Button))]
    [XmlInclude(typeof(Categorizer))]
    [XmlInclude(typeof(ParamSliderAction))]
    [XmlInclude(typeof(ScalerAction))]
    public class Input
    {
        public string label = "";
        [Category("Action")]
        public bool BeginVisible { get; set; }
        private bool ShouldSerializeBeginVisible() { return false; }

        [Category("Action")]
        public bool EndVisible { get; set; }
        private bool ShouldSerializeEndVisible() { return false; }

        [Category("Appearance")]
        public EnabledState Enabled { get; set; }
        private bool ShouldSerializeEnabled() { return false; }

        [ReadOnly(true)]
        public string Target { get; set; }

        public string name;

        public Input()
        {
            BeginVisible = true;
            EndVisible = false;
            Enabled = EnabledState.Enabled;
        }
        public Input(string name) : base()
        {
            this.name = name;
        }
    }
}
