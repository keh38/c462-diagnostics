namespace Audiometer
{
    public class AudiometerSettings
    {
        public float Duration { get; set; }
        public int NumPulses { get; set; }
        public float PipInterval { get; set; }
        public float Ramp {  get; set; }

        public AudiometerChannel[] Channels { get; set; }

        public AudiometerSettings() { }
    }

    public class AudiometerChannel
    {
        public string Waveform { get; set; }
        public string Transducer { get; set; }
        public string Routing { get; set; }
        public bool Continuous { get; set; }
        public bool Pulsed { get; set; }
        
        public AudiometerChannel() { }
    }
}