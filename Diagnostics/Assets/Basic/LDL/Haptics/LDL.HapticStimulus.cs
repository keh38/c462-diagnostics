using System.ComponentModel;

using KLib.Signals.Modulations;
using KLib.Signals.Waveforms;

using OrderedPropertyGrid;

namespace LDL.Haptics
{
    public enum HapticSource { NONE, Vibration, TENS}

    [TypeConverter(typeof(HapticStimulusConverter))]
    public class HapticStimulus
    {
        //        [PropertyOrder(0)]
        [Browsable(false)]
        public HapticSource Source { get; set; }
        private bool ShouldSerializeSource() { return false; }

        [PropertyOrder(1)]
        public Digitimer TENS { get; set; }
        private bool ShouldSerializeTENS() { return false; }

        [PropertyOrder(1)]
        public Sinusoid Vibration { get; set; }
        private bool ShouldSerializeVibration() { return false; }

        [PropertyOrder(2)]
        public string Location { get; set; }
        private bool ShouldSerializeLocation() { return false; }

        [PropertyOrder(3)]
        [Description("Amplitude in volts")]
        public float Level { get; set; }
        private bool ShouldSerializeLevel() { return false; }

        //[PropertyOrder(3)]
        [Browsable(false)]
        public float Delay_ms { get; set; }
        private bool ShouldSerializeDelay_ms() { return false; }

        [PropertyOrder(4)]
        public float Duration_ms { get; set; }
        private bool ShouldSerializeDuration_ms() { return false; }

        [PropertyOrder(5)]
        public AM Envelope { get; set; }
        private bool ShouldSerializeEnvelope() { return false; }

        [ReadOnly(true)]
        [PropertyOrder(6)]
        public string Variable { get; set; }
        private bool ShouldSerializeVariable() { return false; }

        [PropertyOrder(7)]
        public string Expression { get; set; }
        private bool ShouldSerializeExpression() { return false; }

        public HapticStimulus()
        {
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

            Variable = "Delay_ms";
            Expression = "-100:20:100";
        }
    }
}