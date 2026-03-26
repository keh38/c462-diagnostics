using System;
using System.Collections.Generic;
//using System.Drawing.Design;

using KLib.Signals;

using BasicMeasurements;
using LDL.Haptics;

using C462.Shared;

namespace LDL
{
    public class LDLMeasurementSettings : BasicMeasurementConfiguration
    {
        public string Title { get; set; }
        private bool ShouldSerializeTitle() { return false; }

        //[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Prompt { get; set; }
        private bool ShouldSerializePrompt() { return false; }

        public int PromptFontSize {  get; set; }
        private bool ShouldSerializePromptFontSize() { return false; }

        public Audiograms.TestEar TestEar { get; set; }
        private bool ShouldSerializeTestEar() { return false; }

        public float[] TestFrequencies { get; set; }
        private bool ShouldSerializeTestFrequencies() { return false; }

        public int NumRepeats { get; set; }
        private bool ShouldSerializeNumRepeats() { return false; }

        public bool Merge { set; get; }
        private bool ShouldSerializeMerge() { return false; }

        public bool LogSliderTracks { set; get; }
        private bool ShouldSerializeLogSliderTracks() { return false; }


        public float Bandwidth { set; get; }
        private bool ShouldSerializeBandwidth() { return false; }

        public float ToneDuration { set; get; }
        private bool ShouldSerializeToneDuration() { return false; }

        public float ToneDelay { set; get; }
        private bool ShouldSerializeToneDelay() { return false; }

        public float ISI_ms { set; get; }
        private bool ShouldSerializeISI_ms() { return false; }

        public float Ramp { set; get; }
        private bool ShouldSerializeRamp() { return false; }

        public float MinLevel { set; get; }
        private bool ShouldSerializeMinLevel() { return false; }

        public LevelUnits LevelUnits { set; get; }
        private bool ShouldSerializeLevelUnits() { return false; }

        public float ModDepth_pct {  set; get; }
        private bool ShouldSerializeModDepth_pct() { return false; } 

        public HapticStimulus HapticStimulus { set; get; }
        private bool ShouldSerializeHapticStimulus() { return false; }

        public LDLMeasurementSettings() : base()
        {
            Title = "How loud does it sound?";

            TestEar = Audiograms.TestEar.Both;
            TestFrequencies = new float[] { 1000, 2000, 4000 };

            Merge = true;
            LogSliderTracks = false;

            Ramp = 5f;
            ToneDelay = 0;
            ToneDuration = 200;
            Bandwidth = 0;
            ISI_ms = 400;
            ModDepth_pct = 0;
            MinLevel = 10;
            LevelUnits = LevelUnits.dB_SPL;
            NumRepeats = 3;

            Prompt = "Move sliders until sound is uncomfortable";
            PromptFontSize = 72;

            HapticStimulus = new HapticStimulus();
        }

    }
}
