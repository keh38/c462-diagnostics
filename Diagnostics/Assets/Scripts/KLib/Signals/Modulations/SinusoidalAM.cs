using UnityEngine;
using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace KLib.Signals.Modulations
{
    [System.Serializable]
	[ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
	[JsonObject(MemberSerialization.OptOut)]
	public class SinusoidalAM : AM
    {
        [ProtoMember(1, IsRequired = true)]
        public float Frequency_Hz = 40;
        [ProtoMember(2, IsRequired = true)]
        public float Depth = 1;
        [ProtoMember(3, IsRequired = true)]
        public float Phase_cycles = 0.75f;

        private float _lastFreq;
		private float _lastDepth;
		private float _lastPhase;
        private float _phase;
        private float _delayPhase;

        public SinusoidalAM() : this(40, 1)
        {
        }
        
        public SinusoidalAM(float frequency_Hz, float depth)
        {
            _shape = Enumerations.AMShape.Sinusoidal;
            _shortName = "SAM";

            this.Frequency_Hz = _lastFreq = frequency_Hz;
            this.Depth = _lastDepth = depth;
            Phase_cycles = 0;
        }

        [ProtoIgnore]
        public float CurrentPhase { get { return _phase; } }

        public override List<string> GetSweepableParams()
        {
            return new List<string>()
            {
                "SAM.Freq_Hz",
                "SAM.Depth",
                "SAM.Phase"
            };
        }

        public override Action<float> GetParamSetter(string paramName)
		{
			Action<float> setter = null;
			switch (paramName)
			{
			    case "Freq_Hz":
				    setter = x => this.Frequency_Hz = x;
				    break;
                case "Depth":
                    setter = x => this.Depth = x;
                    break;
                case "Depth_dB":
                    setter = x => this.Depth = Mathf.Pow(10, x / 20);
                    break;
                case "Phase":
                    setter = x => this.Phase_cycles = x;
                    break;
            }

            return setter;
		}

        public override void ResetSweepables()
        {
            _lastFreq = this.Frequency_Hz;
            _lastDepth = this.Depth;
        }

        override public List<string> GetValidParameters()
        {
            List<string> p = new List<string>();

            p.Add("SAM.Freq_Hz");
            p.Add("SAM.Depth");
            p.Add("SAM.Depth_dB");
            p.Add("SAM.Phase");
            return p;
        }

        override public string SetParameter(string paramName, float value)
        {
            switch (paramName)
            {
                case "Freq_Hz":
                    Frequency_Hz = value;
                    break;

                case "Depth":
                    Depth = value;
                    break;

                case "Depth_dB":
                    Depth = Mathf.Pow(10, value / 20);
                    break;

                case "Phase":
                    Phase_cycles = value;
                    break;
            }

            return "";
        }

        override public float GetParameter(string paramName)
        {
            switch (paramName)
            {
                case "Freq_Hz":
                    return Frequency_Hz;

                case "Depth":
                    return Depth;

                case "Depth_dB":
                    return 20*Mathf.Log10(Depth);

                case "Phase":
                    return Phase_cycles;
            }

            return float.NaN;
        }


        public override bool Initialize(float Fs, int N, float delay_ms)
        {
            base.Initialize(Fs, N, delay_ms);

            _lastFreq = Frequency_Hz;
			_lastDepth = Depth;

            float cyclePeriod_ms = 1000 / Frequency_Hz;
            _delayPhase = (delay_ms % cyclePeriod_ms) / cyclePeriod_ms;

            _lastPhase = Phase_cycles;
            _phase = 2 * Mathf.PI * (0.75f + _lastPhase - _delayPhase);

            return true;
        }

        public override void ResetPhase(float delay_ms)
        {
            float cyclePeriod_ms = 1000 / Frequency_Hz;
            float newDelayPhase = (delay_ms % cyclePeriod_ms) / cyclePeriod_ms;
            float deltaPhase = newDelayPhase - _delayPhase;
            _delayPhase = newDelayPhase;

            _lastPhase -= deltaPhase;
        }

        public override float Apply(float[] data)
        {
            float df = Frequency_Hz - _lastFreq;
            float t = 0;
            float Theta = 0;
			float deltaDepth = (Depth - _lastDepth) / (float)_npts;
            float deltaPhase = 2 * Mathf.PI * (Phase_cycles - _lastPhase) / _npts;

            if (Depth < 0 || Depth > 1)
                throw new System.Exception("Modulation depth out of range.");

            /*Y(t) = sin(Theta(t) + PhaseIn)
            where Theta(t) = 2pi*[Fi*t + deltaF*t^2/(2T)]
            to give f(t) = Fi + dF *t/T 
            PhaseOut = Theta(T) + PhaseIn*/

            float phaseOffset = 0;
            for (int k = 0; k < _npts; k++)
            {
                if (df!=0 || _lastFreq>0)
                {
                    //float sf = (_lastDepth * Mathf.Sin(Theta + _phase + (2 * Mathf.PI * Phase_cycles)) + 1) / (_lastDepth + 1);
                    float sf = (_lastDepth * Mathf.Sin(Theta + _phase + phaseOffset) + 1) / (_lastDepth + 1);
                    data[k] *= sf;
                }

                _lastDepth += deltaDepth;
                t += _dt;
                Theta = 2 * Mathf.PI * (_lastFreq * t + df * t * t / (2 * _T));
                phaseOffset += deltaPhase;
            }

			_lastFreq = Frequency_Hz;
            _lastPhase = Phase_cycles;
            _phase += Theta + phaseOffset;

            return LevelCorrection;
        }

        [ProtoIgnore]
        public override float LevelCorrection
        {
            get
            {
                float refCorrection = 0;
                if (ApplyLevelCorrection)
                {
                    float modDCVolt = 0;
                    float refVolt = 0.5f;
                    float modPower = 1 + 2 * Depth * modDCVolt + Depth * Depth * refVolt;

                    refCorrection = 20 * Mathf.Log10(Mathf.Sqrt(modPower) / (1 + Depth));
                }

                return refCorrection;
            }
        }

    }
}
