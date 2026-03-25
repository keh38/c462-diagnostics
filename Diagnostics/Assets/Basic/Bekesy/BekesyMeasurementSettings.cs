using System;

using BasicMeasurements;

namespace Bekesy
{
    public class BekesyMeasurementSettings : BasicMeasurementConfiguration
    {
        public string Title { get; set; }
        private bool ShouldSerializeTitle() { return false; }

        public Audiograms.TestEar TestEar { get; set; }
        private bool ShouldSerializeTestEar() { return false; }

        public float[] TestFrequencies { get; set; }
        private bool ShouldSerializeTestFrequencies() { return false; }

        public int NumReversals { get; set; }
        private bool ShouldSerializeNumReversals() { return false; }

        public float StartLevel { get; set; }
        private bool ShouldSerializeStartLevel() { return false; }
        
        public bool Merge { set; get; }
        private bool ShouldSerializeMerge() { return false; }

        public float Ramp { set; get; }
        private bool ShouldSerializeRamp() { return false; }

        public float ToneDuration { set; get; }
        private bool ShouldSerializeToneDuration() { return false; }

        public float ModDepth { set; get; }
        private bool ShouldSerializeModDepth() { return false; }

        public float ModRate { set; get; }
        private bool ShouldSerializeModRate() { return false; }

        public float IPI_ms { set; get; }
        private bool ShouldSerializeIPI_ms() { return false; }

        public bool Continuous { set; get; }
        private bool ShouldSerializeContinuous() { return false; }

        public float AttenuationRate { set; get; }
        private bool ShouldSerializeAttenuationRate() { return false; }

        public BekesyMeasurementSettings() : base()
        {
            Title = "The Softest Sound";

            TestEar = Audiograms.TestEar.Both;
            TestFrequencies = new float[] { 750, 1000, 1500, 2000, 3000, 4000, 8000, 125, 250, 500 };

            Merge = false;
            Ramp = 25f;
            ToneDuration = 1000f;
            IPI_ms = 50;
            ModDepth = 0;
            Continuous = true;
            AttenuationRate = 2.5f;
            NumReversals = 4;

            StartLevel = 50;
        }

    }
}
