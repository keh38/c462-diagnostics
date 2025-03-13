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
	[ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
	[JsonObject(MemberSerialization.OptOut)]
	public class FM : Waveform
    {
        [ProtoMember(1, IsRequired = true)]
        public float Carrier_Hz=500;
        [ProtoMember(2, IsRequired = true)]
        public float ModFreq_Hz=100;
        [ProtoMember(3, IsRequired = true)]
        public float Depth_Hz=2;
        [ProtoMember(4, IsRequired = true)]
        public float Phase_cycles=0;

		[ProtoIgnore]
		[JsonIgnore]
        public bool UseLUT;

		private float lastFmod;
		private float lastDepth;

		private float modArg;
		private float mainArg;

        private float[] LUT;
        private int skipFactor;
        private int phaseIndex;
        private int intFs;
        private int lastSkip;

        public FM()
        {
			Carrier_Hz = 500;
            Phase_cycles = 0;
            UseLUT = false;

            shape = Waveshape.FM;
            _shortName = "FM";
        }

        new public static List<SweepableParam> GetSweepableParams()
		{
			List<SweepableParam> par = new List<SweepableParam>();
			par.Add(new SweepableParam("FM ModFreq", "Hz", 2));
			par.Add(new SweepableParam("FM Depth", "Hz", 50));

			return par;
		}

		override public Action<float> ParamSetter(string paramName)
		{
			Action<float> setter = null;
			switch (paramName)
			{
			case "FM ModFreq":
				setter = x => this.ModFreq_Hz = x;
				break;
			case "FM Depth":
				setter = x => this.Depth_Hz = x;
				break;
			}

			return setter;
		}

        override public void ResetSweepables()
        {
            lastFmod = this.ModFreq_Hz;
            lastDepth = this.Depth_Hz;
        }

        override public string SetParameter(string paramName, float value)
        {
            switch (paramName)
            {
                case "CarrierHz":
                    this.Carrier_Hz = value;
                    break;
                case "ModFreqHz":
                    this.ModFreq_Hz = value;
                    break;
                case "DepthHz":
                    this.Depth_Hz = value;
                    break;
                case "PhaseCyc":
                    this.Phase_cycles = value;
                    break;
            }

            return "";
        }

        override public List<string> GetValidParameters()
        {
            List<string> plist = new List<string>();
            plist.Add("CarrierHz");
            plist.Add("ModFreqHz");
            plist.Add("DepthHz");
            plist.Add("PhaseCyc");
            return plist;
        }

        override public void Initialize(float Fs, int N, int Nmax, Level level)
        {
            base.Initialize(Fs, N, level);

            intFs = (int)Fs;
            LUT = new float[intFs];

            for (int k = 0; k < intFs; k++)
            {
                LUT[k] = (float)(Mathf.Sin(2.0f * Mathf.PI * (float)k / Fs));
            }

            phaseIndex = 0;
            skipFactor = (int)Carrier_Hz;
            lastSkip = skipFactor;

			lastDepth = Depth_Hz;
			lastFmod = ModFreq_Hz;

			mainArg = 0;
            modArg = 2*Mathf.PI * Phase_cycles;
        }

        public override float GetMaxLevel(Level level, float Fs)
        {
            return (level.Cal == null) ? float.NaN : level.Cal.GetMax(Carrier_Hz);
        }

        override public References Create(float[] data)
        {
            for (int k = 0; k < Npts; k++)
            {
                modArg += 2 * Mathf.PI * dt * ModFreq_Hz;
                if (modArg > 2 * Mathf.PI) modArg -= 2 * Mathf.PI;

                float v1 = Depth_Hz / ModFreq_Hz * Mathf.Sin(modArg);
                mainArg += 2 * Mathf.PI * dt * (Carrier_Hz + v1);
                if (mainArg > 2 * Mathf.PI) mainArg -= 2 * Mathf.PI;

                data[k] = Mathf.Cos(mainArg);
            }

            return new References(_calib.GetReference(Carrier_Hz),
                                  _calib.GetMax(Carrier_Hz));
        }

        public References CreateContinuous(float[] data)
        {
            float deltaFm = (ModFreq_Hz - lastFmod) / (float)Npts;
            float deltaDepth = (Depth_Hz - lastDepth) / (float)Npts;

            for (int k = 0; k < Npts; k++)
            {
                data[k] = Mathf.Sin(mainArg);

                lastFmod += deltaFm;
                lastDepth += deltaDepth;

                modArg += 2 * Mathf.PI * dt * lastFmod;
                if (modArg > 2 * Mathf.PI) modArg -= 2 * Mathf.PI;

                mainArg += 2 * Mathf.PI * dt * (Carrier_Hz + lastDepth * Mathf.Cos(modArg));
                if (mainArg > 2 * Mathf.PI) mainArg -= 2 * Mathf.PI;
            }

            lastFmod = ModFreq_Hz;
            lastDepth = Depth_Hz;

            return new References(_calib.GetReference(Carrier_Hz),
                                  _calib.GetMax(Carrier_Hz));
        }

    }
}
