using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Pupillometry
{
    public class DynamicRangeSettings
    {
        [Description("Duration of baseline period before stimulation (seconds)")]
        public float PrestimulusBaseline { get; set; }
        private bool ShouldSerializePrestimulusBaseline() { return false; }

        [Description("Duration of baseline period after stimulation (seconds)")]
        public float PoststimulusBaseline { get; set; }
        private bool ShouldSerializePoststimulusBaseline() { return false; }

        [Description("Duration of light modulation cycle (seconds)")]
        public float StimulusPeriod { get; set; }
        private bool ShouldSerializeStimulusPeriod() { return false; }

        [Description("Number of light modulation cycles")]
        public int NumRepetitions { get; set; }
        private bool ShouldSerializeNumRepetitions() { return false; }

        [Category("LED Intensity")]
        [DisplayName("Min")]
        [Description("Minimum intensity of LEDs if present (0-1)")]
        public float MinLEDIntensity { get; set; }
        private bool ShouldSerializeMinLEDIntensity() { return false; }

        [Category("LED Intensity")]
        [DisplayName("Max")]
        [Description("Maximum intensity of LEDs if present (0-1)")]
        public float MaxLEDIntensity { get; set; }
        private bool ShouldSerializeMaxLEDIntensity() { return false; }

        [Category("Screen Intensity")]
        [DisplayName("Min")]
        [Description("Minimum screen intensity")]
        public float MinScreenIntensity { get; set; }
        private bool ShouldSerializeMinScreenIntensity() { return false; }


        [Category("Screen Intensity")]
        [DisplayName("Max")]
        [Description("Maximum screen intensity")]
        public float MaxScreenIntensity { get; set; }
        private bool ShouldSerializeMaxScreenIntensity() { return false; }


        public DynamicRangeSettings()
        {
            PrestimulusBaseline = 2;
            PoststimulusBaseline = 2;
            StimulusPeriod = 20;
            NumRepetitions = 4;
            MinLEDIntensity = 0;
            MaxLEDIntensity = 1;
            MinScreenIntensity = 0;
            MaxScreenIntensity = 1;
        }
    }
}