using System;
using System.Collections.Generic;

using KLib.Signals;

using BasicMeasurements;
using LDL.Haptics;

using C462.Shared;
using Audiograms;
using Newtonsoft.Json;

namespace Tapping
{
    [JsonObject(MemberSerialization.OptOut)]
    public class TappingConfiguration : BasicMeasurementConfiguration
    {
        public string Title { get; set; }
        public TestEar TestEar { get; set; }
        public Channel Channel { get; set; }
        public float MinISI { get; set; }
        public int PatternLength { get; set; }
        public int NumRepeats { get; set; }
        public string IntervalExpression { get; set; }

        public TappingConfiguration() : base()
        {
            Title = "Tapping";

            TestEar = Audiograms.TestEar.Both;

            Channel = new Channel();
            MinISI = 0.5f;
            PatternLength = 5;
            IntervalExpression = "[1 2 3]";
            NumRepeats = 5;
        }

    }
}
