using System;
using System.Xml.Serialization;

namespace Pupillometry
{
    public class DynamicRangeSettings
    {
        public string Name { get; set; }
        private bool ShouldSerializeName() { return false; }

        public float PrestimulusBaseline { get; set; }
        private bool ShouldSerializePrestimulusBaseline() { return false; }

        public float PoststimulusBaseline { get; set; }
        private bool ShouldSerializePoststimulusBaseline() { return false; }

        public float StimulusPeriod { get; set; }
        private bool ShouldSerializeStimulusPeriod() { return false; }

        public int NumRepetitions { get; set; }
        private bool ShouldSerializeNumRepetitions() { return false; }

        public float MinLEDIntensity { get; set; }
        private bool ShouldSerializeMinLEDIntensity() { return false; }

        public float MaxLEDIntensity { get; set; }
        private bool ShouldSerializeMaxLEDIntensity() { return false; }

        public int FixationPointSize { get; set; }
        private bool ShouldSerializeFixationPointSize() { return false; }

        public float MinScreenIntensity { get; set; }
        private bool ShouldSerializeMinScreenIntensity() { return false; }


        public float MaxScreenIntensity { get; set; }
        private bool ShouldSerializeMaxScreenIntensity() { return false; }


        public DynamicRangeSettings()
        {
            Name = "Defaults";
            PrestimulusBaseline = 2;
            PoststimulusBaseline = 2;
            StimulusPeriod = 20;
            NumRepetitions = 4;
            MinLEDIntensity = 0;
            MaxLEDIntensity = 1;
            MinScreenIntensity = 0;
            MaxScreenIntensity = 1;
            FixationPointSize = 50;
        }
    }
}