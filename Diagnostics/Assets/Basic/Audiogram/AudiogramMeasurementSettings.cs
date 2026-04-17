using System;

using BasicMeasurements;

namespace Audiograms
{
    public enum TestEar { Left, Right, Both }

    public class AudiogramMeasurementSettings : BasicMeasurementConfiguration
    {
        public string Title { get; set; }
        public int ButtonWidth { get; set; }
        public int ButtonHeight { get; set; }
        public int ButtonFontSize { get; set; }
        public bool ShowOtherEarMessage { get; set; }
        public bool ShowNewFrequencyMessage { get; set; }
        public float MinISI { get; set; }
        public float MaxISI { get; set; }
        public TestEar TestEar { get; set; }
        public float[] TestFrequencies { get; set; }
        public bool Abridged { get; set; }
        public bool Merge { set; get; }
        public float Ramp { set; get; }
        public float ToneDuration { set; get; }
        public float ModDepth { set; get; }
        public float ModRate { set; get; }
        public float IPI_ms { set; get; }
        public int NumPips { set; get; }
        public float MinValidResponseTime { set; get; }
        public float MaxValidResponseTime { set; get; }
        public float MaxMaskerSPL { set; get; }

        //[Category("Sequence")]
        //[Description("If false, level is tracked in dB HL")]
        //public bool TrackInSPL = false;

        public AudiogramMeasurementSettings() : base()
        {
            Title = "The Softest Sound";

            TestEar = TestEar.Both;
            TestFrequencies = new float[] { 750, 1000, 1500, 2000, 3000, 4000, 8000, 125, 250, 500 };

            Merge = false;
            Ramp = 25f;
            ToneDuration = 1000f;
            NumPips = 1;
            IPI_ms = 50;
            MaxMaskerSPL = 80f;
            MinValidResponseTime = 0.18f;
            MaxValidResponseTime = 2.5f;
            MinISI = 3;
            MaxISI = 7;
            Abridged = false;
            ModDepth = 0;
            ModRate = 1;
            ShowOtherEarMessage = true;
            ShowNewFrequencyMessage = true;
            ButtonWidth = 500;
            ButtonHeight = 150;
            ButtonFontSize = 60;
        }

    }
}
