using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;

using KLib.Signals;

namespace Turandot.Inputs
{
    [JsonObject(MemberSerialization.OptOut)]
    public class Button : Input
    {
        public float Delay_ms { get; set; }
        private bool ShouldSerializeDelay_ms() { return false; }

        public int NumFlash { get; set; }
        private bool ShouldSerializeNumFlash() { return false; }

        public float Duration_ms { get; set; }
        private bool ShouldSerializeDuration_ms() { return false; }

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
