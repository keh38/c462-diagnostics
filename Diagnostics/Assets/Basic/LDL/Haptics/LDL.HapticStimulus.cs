using System.Collections.Generic;

using KLib.Signals.Modulations;
using KLib.Signals.Waveforms;

using Newtonsoft.Json;

namespace LDL.Haptics
{
    public enum HapticSource { NONE, Vibration, TENS}

    [JsonObject(MemberSerialization.OptOut)]
    public class HapticStimulus
    {
        //        [PropertyOrder(0)]
        public HapticSource Source { get; set; }
        private bool ShouldSerializeSource() { return false; }

        public bool SaveLDLGram { get; set; }
        private bool ShouldSerializeSaveLDLGram() { return false; }

        public bool DoAudioOnly { get; set; }
        private bool ShouldSerializeDoAudioOnly() { return false; }

        public Digitimer TENS { get; set; }
        private bool ShouldSerializeTENS() { return false; }

        public Sinusoid Vibration { get; set; }
        private bool ShouldSerializeVibration() { return false; }

        public string Location { get; set; }
        private bool ShouldSerializeLocation() { return false; }

        public float Level { get; set; }
        private bool ShouldSerializeLevel() { return false; }

        public float Delay_ms { get; set; }
        private bool ShouldSerializeDelay_ms() { return false; }

        public float Duration_ms { get; set; }
        private bool ShouldSerializeDuration_ms() { return false; }

        public AM Envelope { get; set; }
        private bool ShouldSerializeEnvelope() { return false; }

        public List<HapticSeqVar> SeqVars { get; set; }
        private bool ShouldSerializeSeqVars() { return false; }

        public HapticStimulus()
        {
            SaveLDLGram = false;
            DoAudioOnly = true;
            Source = HapticSource.NONE;
            TENS = new Digitimer();
            Vibration = new Sinusoid()
            {
                Frequency_Hz = 175
            };
            Level = 1;
            Delay_ms = 0;
            Duration_ms = 200;
            Envelope = new AM();
            SeqVars = new List<HapticSeqVar>();

        }
    }
}