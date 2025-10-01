using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

using Newtonsoft.Json;

using OrderedPropertyGrid;
using KLib.TypeConverters;
using Turandot.TypeConverters;
using KLib.Signals;

namespace Turandot.Inputs
{
    [JsonObject(MemberSerialization.OptOut)]
    [TypeConverter(typeof(ButtonActionTypeConverter))]
    public class Button : Input
    {
        [Category("Behavior")]
        [DisplayName("Delay")]
        [Description("Delay in ms")]
        [PropertyOrder(0)]
        public float Delay_ms { get; set; }
        private bool ShouldSerializeDelay_ms() { return false; }

        [Category("Behavior")]
        [PropertyOrder(1)]
        public int NumFlash { get; set; }
        private bool ShouldSerializeNumFlash() { return false; }

        [Category("Behavior")]
        [DisplayName("Duration")]
        [Description("Duration in ms")]
        [PropertyOrder(2)]
        public float Duration_ms { get; set; }
        private bool ShouldSerializeDuration_ms() { return false; }

        [Category("Behavior")]
        [DisplayName("Interval")]
        [Description("Interval in ms")]
        [PropertyOrder(3)]
        [TypeConverter(typeof(FloatTypeConverter))]
        public float Interval_ms { get; set; }
        private bool ShouldSerializeInterval_ms() { return false; }

        public bool tweenScale = false;
        public float scaleTo = 2;

        public Button() : base()
        {
            Delay_ms = 0;
            NumFlash = 0;
            Duration_ms = 500;
            Interval_ms = 1000;
            
        }
    }
}
