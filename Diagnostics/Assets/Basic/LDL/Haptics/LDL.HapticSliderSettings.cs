using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace LDL.Haptics
{
    [JsonObject(MemberSerialization.OptOut)]
    public class HapticSliderSettings : LDL.SliderSettings
    {
        public PropValPairList propValPairs;

        public HapticSliderSettings() { }
        public new HapticSliderSettings Clone()
        { 
            HapticSliderSettings clone = new HapticSliderSettings();
            clone.ear = this.ear;
            clone.Freq_Hz = this.Freq_Hz;
            clone.var = this.var;
            clone.min = this.min;
            clone.max = this.max;
            clone.start = this.start;
            clone.end = this.end;
            clone.isMaxed = this.isMaxed;
            clone.propValPairs = this.propValPairs.Clone(); 
            return clone;
        }
    }
}