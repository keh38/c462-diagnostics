using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KLib.Signals.Enumerations;

namespace KLib.Signals.Waveforms
{
    public static class WaveformFactory
    {
        public static List<Type> WaveformTypes()
        {
            List<Type> wfTypes = new List<Type>();
            wfTypes.Add(typeof(Waveform));
            wfTypes.Add(typeof(Sinusoid));
            wfTypes.Add(typeof(Noise));
            wfTypes.Add(typeof(ToneCloud));
            wfTypes.Add(typeof(MovingRippleNoise));
            wfTypes.Add(typeof(FM));
            wfTypes.Add(typeof(UserFile));
            wfTypes.Add(typeof(RippleNoise));

            return wfTypes;
        }

        public static List<Waveform> OneOfEach()
        {
            List<Waveform> wf = new List<Waveform>();
            foreach (Type t in WaveformTypes())
            {
                wf.Add(Create(t));
            }
            return wf;
        }

        public static Waveform Create(Type t)
        {
            Waveform wf = null;
            if (t == typeof(Waveform))
            {
                wf = new Waveform();
            }
            else if (t == typeof(Sinusoid))
            {
                wf = new Sinusoid();
            }
            else if (t == typeof(Noise))
            { 
                wf = new Noise();
            }
            else if (t == typeof(ToneCloud))
            {
                wf = new ToneCloud();
            }
            else if (t == typeof(MovingRippleNoise))
            {
                wf = new MovingRippleNoise();
            }
            else if (t == typeof(FM))
            {
                wf = new FM();
            }
            else if (t == typeof(UserFile))
            {
                wf = new UserFile();
            }
            else if (t == typeof(RippleNoise))
            {
                wf = new RippleNoise();
            }
            else
            {
                throw new System.Exception("Invalid waveform type");
            }
            return wf;
        }

        public static Waveform Create(Waveshape type)
        {
            Waveform wf = null;
            switch (type)
            {
                case Waveshape.Sinusoid:
                    wf = new Sinusoid();
                    break;
                case Waveshape.Noise:
                    wf = new Noise();
                    break;
                case Waveshape.ToneCloud:
                    wf = new ToneCloud();
                    break;
                case Waveshape.MovingRippleNoise:
                    wf = new MovingRippleNoise();
                    break;
                case Waveshape.FM:
                    wf = new FM();
                    break;
                case Waveshape.File:
                    wf = new UserFile();
                    break;
                case Waveshape.RippleNoise:
                    wf = new RippleNoise();
                    break;
            }
            return wf;
        }

    }
}
