using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Cues
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    [XmlInclude(typeof(FixationPointAction))]
    [XmlInclude(typeof(ImageAction))]
    [XmlInclude(typeof(Message))]
    [XmlInclude(typeof(ProgressBarAction))]
    [XmlInclude(typeof(Help))]
    [XmlInclude(typeof(CounterAction))]
    [XmlInclude(typeof(ScoreboardAction))]
    [XmlInclude(typeof(VideoAction))]
    public class Cue
    {
        public int color = 0xFFFFFF;

        [Category("Action")]
        public bool BeginVisible { get; set; }
        private bool ShouldSerializeBeginVisible() { return false; }

        [Category("Action")]
        public bool EndVisible { get; set; }
        private bool ShouldSerializeEndVisible() { return false; }

        [ReadOnly(true)]
        public string Target { get; set; }
        //private bool ShouldSerializeTarget() { return false; }

        //public float delay_ms = 0f;
        //public float duration_ms = 200f;
        //public float interval_ms = 500f;
        //public int numFlash = 0;
        //public bool changeAppearance = false;
        public int X = 0;
        public int Y = 0;

        public Cue() { }

        [XmlIgnore]
        [ProtoIgnore]
        [JsonIgnore]
        [Browsable(false)]
        virtual public string Name
        {
            get { return "LED"; }
        }

        [JsonIgnore]
        [Browsable(false)]
        public float A
        {
            get { return ((color & 0xFF000000) >> 24) / 255f; }
        }

        [JsonIgnore]
        [Browsable(false)]
        public float R
        {
            get { return ((color & 0xFF0000) >> 16) / 255f; }
        }

        [JsonIgnore]
        [Browsable(false)]
        public float G
        {
            get { return ((color & 0x00FF00) >> 8) / 255f; }
        }

        [JsonIgnore]
        [Browsable(false)]
        public float B
        {
            get { return (color & 0xFF) / 255f; }
        }

        [JsonIgnore]
        [Browsable(false)]
        virtual public bool IsSequenceable
        {
            get { return false; }
        }

        virtual public List<string> GetPropertyNames()
        {
            return new List<string>();
        }

        virtual public string SetProperty(string property, float value)
        {
            return "";
        }

    }
}
