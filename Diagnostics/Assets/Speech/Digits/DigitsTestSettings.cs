using System;


using BasicMeasurements;
//using System.Drawing.Design;
using Unity.VisualScripting;

namespace DigitsTest
{
    public enum BlockOrder { Sequential, BlockedSequential, Interleaved }

    public class DigitsTestSettings : BasicMeasurementConfiguration
    {
        public new bool UseDefaultInstructions { get; set; }

        public new string InstructionMarkdown { get; set; }

        public string Title { get; set; }
        private bool ShouldSerializeTitle() { return false; }

        public bool ApplyIllumination { get; set; }
        private bool ShouldSerializeApplyIllumination() { return false; }

        public int NumPracticeTrials { get; set; }
        private bool ShouldSerializeNumPracticeTrials() { return false; }

        public int NumTestBlocksPerCondition { get; set; }
        private bool ShouldSerializeNumTestBlocksPerCondition() { return false; }

        public int NumTestTrialsPerBlock { get; set; }
        private bool ShouldSerializeNumTestTrialsPerBlock() { return false; }

        public BlockOrder BlockOrder { get; set; }
        private bool ShouldSerializeBlockOrder() { return false; }

        public float SpeakerLevel { get; set; }
        private bool ShouldSerializeSpeakerLevel() { return false; }

        public string SNR { get; set; }
        private bool ShouldSerializeSNR() { return false; }

        public float MinDelay { get; set; }
        private bool ShouldSerializeMinDelay() { return false; }

        public float MaxDelay { get; set; }
        private bool ShouldSerializeMaxDelay() { return false; }

        public float RampStart { get; set; }
        private bool ShouldSerializeRampStart() { return false; }

        public float RampEnd { get; set; }
        private bool ShouldSerializeRampEnd() { return false; }

        public float RampStep { get; set; }
        private bool ShouldSerializeRampStep() { return false; }

        public DigitsTestSettings() : base()
        {
            Title = "Digits Test";
            ApplyIllumination = true;
            InstructionFontSize = 60;

            NumPracticeTrials = 3;
            NumTestBlocksPerCondition = 1;
            NumTestTrialsPerBlock = 10;
            BlockOrder = BlockOrder.Interleaved;
            SpeakerLevel = 65f;
            SNR = "0";

            MinDelay = 1;
            MaxDelay = 1;

            RampStart = 9;
            RampEnd = 0;
            RampStep = 4.5f;
        }

    }
}
