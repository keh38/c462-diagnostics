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

        public float CurrentPhase { get { return _phase; } }

        public override List<SweepableParam> GetSweepableParams()
        {
            List<SweepableParam> par = new List<SweepableParam>();
            par.Add(new SweepableParam("ModFreq", "Hz", 40));
            par.Add(new SweepableParam("ModDepth", "0-1", 1));
            par.Add(new SweepableParam("ModDepth_dB", "dB", 1));
            par.Add(new SweepableParam("ModPhase", "cycles", 1));

            return par;
        }

        public override Action<float> ParamSetter(string paramName)
		{
			Action<float> setter = null;
			switch (paramName)
			{
			    case "ModFreq":
				    setter = x => this.Frequency_Hz = x;
				    break;
                case "ModDepth":
                    setter = x => this.Depth = x;
                    break;
                case "ModDepth_dB":
                    setter = x => this.Depth = Mathf.Pow(10, x / 20);
                    break;
                case "ModPhase":
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

        public override bool Initialize(float Fs, int N)
        {
            base.Initialize(Fs, N);

            _lastFreq = Frequency_Hz;
			_lastDepth = Depth;
            _lastPhase = Phase_cycles;
            _phase = 2 * Mathf.PI * (0.75f + Phase_cycles);

            return true;
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
