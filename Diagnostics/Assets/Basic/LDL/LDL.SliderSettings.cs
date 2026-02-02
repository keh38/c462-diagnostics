using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace LDL
{
    [JsonObject(MemberSerialization.OptOut)]
    public class SliderSettings
    {
        public KLib.Signals.Laterality ear;
        public float Freq_Hz;
        public string var;
        public float min;
        public float max;
        public float start;
        public float end;
        public bool isMaxed;

        public SliderLog log;

        public SliderSettings() { }
        public SliderSettings Clone()
        {
            SliderSettings clone = new SliderSettings();
            clone.ear = this.ear;
            clone.Freq_Hz = this.Freq_Hz;
            clone.var = this.var;
            clone.min = this.min;
            clone.max = this.max;
            clone.start = this.start;
            clone.end = this.end;
            clone.isMaxed = this.isMaxed;
            clone.log = this.log;

            return clone;
        }
    }
}