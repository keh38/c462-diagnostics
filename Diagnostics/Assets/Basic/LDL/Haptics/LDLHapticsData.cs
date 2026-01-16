using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace LDL.Haptics
{
    [JsonObject(MemberSerialization.OptOut)]
    public class LDLHapticsData
    {
        public Audiograms.AudiogramData LDLgram = null;
        public List<HapticsTestCondition> testConditions;
        public List<HapticSliderSettings> sliderSettings = new List<HapticSliderSettings>();
    }
}