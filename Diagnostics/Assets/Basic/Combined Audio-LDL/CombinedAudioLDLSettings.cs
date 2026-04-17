using System;
using System.Collections.Generic;

using KLib.Signals;

using BasicMeasurements;
using LDL.Haptics;

using C462.Shared;

namespace Audiograms
{
    public class CombinedAudioLDLSettings : BasicMeasurementConfiguration
    {
        public string Title { get; set; }
        public string Prompt { get; set; }
        public int PromptFontSize {  get; set; }
        public Audiograms.TestEar TestEar { get; set; }
        public float[] TestFrequencies { get; set; }
        public int NumRepeats { get; set; }
        public int NumReversals { get; set; }
        public bool Merge { set; get; }
        public bool LogSliderTracks { set; get; }
        public float Bandwidth { set; get; }
        public float ToneDuration { set; get; }
        public float ToneDelay { set; get; }
        public float ISI_ms { set; get; }
        public float Ramp { set; get; }

        public float MinLevel { set; get; }
        public LevelUnits LevelUnits { set; get; }
        public float ModDepth_pct {  set; get; }
        public HapticStimulus HapticStimulus { set; get; }

        public CombinedAudioLDLSettings() : base()
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
            NumRepeats = 1;
            NumReversals = 2;

            PromptFontSize = 72;
        }

    }
}
