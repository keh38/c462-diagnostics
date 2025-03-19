using UnityEngine;
using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals.Calibration;
using KLib.Signals.Enumerations;

namespace KLib.Signals.Waveforms
{
    [System.Serializable]
	[ProtoContract]
	[JsonObject(MemberSerialization.OptIn)]
	public class Sinusoid : Waveform
    {
        [ProtoMember(1, IsRequired = true)]
        [JsonProperty]
        public float Frequency_Hz;

        [ProtoMember(2, IsRequired = true)]
        [JsonProperty]
        public float Phase_cycles;

        private float lastFreq;
        private float phase_radians;

		float deltaArg;

        public Sinusoid()
        {
            Frequency_Hz = lastFreq = 500;
            Phase_cycles = phase_radians = 0;

			//FrequencyRes = 0.5f;
			shape = Waveshape.Sinusoid;
            _shortName = "Tone";
        }

		public override List<SweepableParam> GetSweepableParams()
		{
			List<SweepableParam> par = new List<SweepableParam>();
            par.Add(new SweepableParam("Frequency", "Hz", 500));
            par.Add(new SweepableParam("Phase", "cycles", 0));

            return par;
		}

        override public Action<float> ParamSetter(string paramName)
        {
            Action<float> setter = null;
            switch (paramName)
            {
                case "Frequency":
                    setter = x => this.Frequency_Hz = x;
                    break;
                case "Phase":
                    setter = x => this.Phase_cycles = x;
                    break;
            }

            return setter;
        }

        override public string SetParameter(string paramName, float value)
        {
            switch (paramName)
            {
                case "Frequency_Hz":
                    this.Frequency_Hz = value;
                    break;
                case "Phase_cycles":
                    this.Phase_cycles = value;
                    break;
            }

            return "";
        }

        override public float GetParameter(string paramName)
        {
            switch (paramName)
            {
                case "Frequency_Hz":
                    return Frequency_Hz;
                case "Phase_cycles":
                    return Phase_cycles;
            }

            return float.NaN;
        }

        override public List<string> GetValidParameters()
        {
            List<string> plist = new List<string>();
            plist.Add("Frequency_Hz");
            plist.Add("Phase_cycles");
            return plist;
        }

        override public void ResetSweepables()
        {
            lastFreq = Frequency_Hz;
            phase_radians = 2 * Mathf.PI * Phase_cycles;
        }

        override public void Initialize(float Fs, int N, Channel channel)
        {
            base.Initialize(Fs, N, channel);

            lastFreq = Frequency_Hz;
            phase_radians = 2 * Mathf.PI * Phase_cycles;

            deltaArg = 2*Mathf.PI*Frequency_Hz*dt;
        }

        public void CreateTrig(float[] data)
        {
            float df = (Frequency_Hz - lastFreq) / Npts;
            float newPhaseRadians = 2 * Mathf.PI * Phase_cycles;
            float dphi = (newPhaseRadians - phase_radians) / Npts;

            dphi = 0;
            /*Y(t) = sin(Theta(t) + PhaseIn)
            where Theta(t) = 2pi*[Fi*t + deltaF*t^2/(2T)]
            to give f(t) = Fi + dF *t/T 
            PhaseOut = Theta(T) + PhaseIn*/

            for (int k = 0; k < Npts; k++)
            {
                data[k] = Mathf.Sin(phase_radians);
				lastFreq += df;
				phase_radians += 2 * Mathf.PI * lastFreq * dt + dphi;
				if (phase_radians > 2*Mathf.PI) phase_radians -= 2*Mathf.PI;
			}

			lastFreq = Frequency_Hz;
		}

        public override float GetMaxLevel(Level level, float Fs)
        {
            return (level.Cal==null) ? float.NaN : level.Cal.GetMax(Frequency_Hz);
        }

        override public References Create(float[] data) 
		{
	 	    CreateTrig(data);

            return _calib.GetAllReferences(Frequency_Hz);
        }

    }
}
