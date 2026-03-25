using System;

using BasicMeasurements;

namespace Audiograms
{
    public enum TestEar { Left, Right, Both }

    public class AudiogramMeasurementSettings : BasicMeasurementConfiguration
    {
        public string Title { get; set; }
        private bool ShouldSerializeTitle() { return false; }

        public int ButtonWidth { get; set; }
        private bool ShouldSerializeButtonWidth() { return false; }

        public int ButtonHeight { get; set; }
        private bool ShouldSerializeButtonHeight() { return false; }

        public int ButtonFontSize { get; set; }
        private bool ShouldSerializeButtonFontSize() { return false; }

        public bool ShowOtherEarMessage { get; set; }
        private bool ShouldSerializeShowOtherEarMessage() { return false; }

        public bool ShowNewFrequencyMessage { get; set; }
        private bool ShouldSerializeShowNewFrequencyMessage() { return false; }

        public float MinISI {  get; set; }
        private bool ShouldSerializeMinISI() { return false; }

        public float MaxISI { get; set; }
        private bool ShouldSerializeMaxISI() { return false; }
 
        public TestEar TestEar { get; set; }
        private bool ShouldSerializeTestEar() { return false; }
        
        public float[] TestFrequencies { get; set; }
        private bool ShouldSerializeTestFrequencies() { return false; }

        public bool Abridged { get; set; }
        private bool ShouldSerializeAbridged() { return false; }

        public bool Merge { set; get; }
        private bool ShouldSerializeMerge() { return false; }

        public float Ramp { set; get; }
        private bool ShouldSerializeRamp() { return false; }

        public float ToneDuration { set; get; }
        private bool ShouldSerializeToneDuration() {  return false; }

        public float ModDepth { set; get; }
        private bool ShouldSerializeModDepth() { return false; }

        public float ModRate { set; get; }
        private bool ShouldSerializeModRate() { return false; }

        public float IPI_ms { set; get; }
        private bool ShouldSerializeIPI_ms() {  return false; }

        public int NumPips { set; get;  }
        private bool ShouldSerializeNumPips() {  return false; }

        public float MinValidResponseTime { set; get; }
        private bool ShouldSerializeMinValidResponseTime() {  return false; }

        public float MaxValidResponseTime { set; get; }
        private bool ShouldSerializeMaxValidResponseTime() { return false; }

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
