using System;
using System.Collections.Generic;
using System.ComponentModel;

using KLib;
using KLib.Signals.Enumerations;

namespace LDL
{
    [Serializable]
    public class LDLMeasurementSettings : BasicMeasurementConfiguration
    {
        public float[] testFreqs = { 1000, 2000, 4000 };

        //public Laterality Laterality { set; get; }
        //public LevelUnits Units { set; get; }

        public bool Merge { set; get; }

        public float Ramp { set; get; }

        [Category("Stimulus")]
        public float ToneDuration { set; get; }
        private bool ShouldSerializeToneDuration() { return false; }
        
        public float ISI_ms { set; get; }

        public int NumPips { set; get; }

        public int NumRepeats = 3;

        public float modDepth_pct = 0;
        public float MinLevel = 10;

        public List<string> instructions = null;

        public LDLMeasurementSettings() : base()
        {
            //Laterality = Laterality.Diotic;
            //Units = LevelUnits.dB_SL;
            Merge = true;
            Ramp = 5f;
            ToneDuration = 200;
            NumPips = 4;
            ISI_ms = 400;
        }

    }
}
