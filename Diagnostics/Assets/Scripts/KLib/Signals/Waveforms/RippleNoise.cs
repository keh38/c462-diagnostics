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
	public class RippleNoise : Waveform
    {
        [ProtoMember(1, IsRequired = true)]
        public float Density;
        [ProtoMember(2, IsRequired = true)]
        public float Contrast;
        [ProtoMember(3, IsRequired = true)]
        public float Phase;
        [ProtoMember(4, IsRequired = true)]
        public bool precompensate;
        [ProtoMember(5, IsRequired = true)]
        public int seed;
        [ProtoMember(6, IsRequired = true)]
        public float Fmin;
        [ProtoMember(7, IsRequired = true)]
        public float Fmax;
        [ProtoMember(8, IsRequired = true)]
        public float Duration;

        int curIndex;

		float[] token;
        private static readonly float tokenLength_s = 2.631f;
        private float noiseRMS = 0.25f;
        private float ref_dBV;
        private float ref_dB;
        private float maxLevel;

        public RippleNoise()
        {
			shape = Waveshape.Ripple_Noise;
            _shortName = "RippleNoise";

            Fmin = 350;
            Fmax = 5600;
            Density = 0.5f;
            Contrast = 20;
            Phase = 0;
            precompensate = false;
            seed = 37;
            Duration = tokenLength_s;

            ref_dBV = 20f * Mathf.Log10(noiseRMS);
        }

        public override float GetMaxLevel(Level level, float Fs)
        {
            ComputeReferences(level, Fs);
            return maxLevel;
        }

		new public static List<SweepableParam> GetSweepableParams()
		{
			List<SweepableParam> par = new List<SweepableParam>();
			return par;
		}
		
		override public Action<float> ParamSetter(string paramName)
		{
			Action<float> setter = null;
			return setter;
		}

        override public void Initialize(float Fs, int N, int Nmax, Level level)
        {
			base.Initialize(Fs, N, level);

			CreateToken();
			curIndex = 0;
        }

		private void CreateToken()
		{
            var normrnd = new KLib.Math.GaussianRandom(seed);

            int tokenLength = Mathf.RoundToInt(Duration * samplingRate_Hz);
			token = new float[tokenLength];

            for (int k=0; k<tokenLength; k++)
            {
                token[k] = Mathf.Clamp(normrnd.Next(0, noiseRMS), -1, 1);
            }

            ApplyFilter();

            ComputeReferences(_level, samplingRate_Hz);
		}

        private void ComputeReferences(Level level, float Fs)
        {
            ref_dB = ref_dBV;
            maxLevel = ref_dBV;

            int n = Mathf.RoundToInt(Duration * Fs);
            float df = Fs / n;
            float[] w = ComputeRippleWeights(df, n / 2);

            float[] freq = new float[n / 2];
            float scale = Mathf.Sqrt(2.0f / n) * noiseRMS / KMath.RMS(w);
            for (int k = 0; k < w.Length; k++)
            {
                freq[k] = k * df;
                w[k] *= scale;
            }

            if (precompensate && level.SPLBasedUnits)
            {
                if (level.Cal == null)
                    throw new ApplicationException("Null calibration.");

                level.Cal.GetAmplitudesInterp(freq, w);
            }
            ref_dB = level.Cal.GetWeightedReference(w, Fs);
            maxLevel = level.Cal.GetWeightedMax(w, Fs);

            if (level.Reference == LevelReference.Spectrum_level)
            {
                float spectrumLevelFactor = 10 * Mathf.Log10(KMath.SumOfSquares(w));
                ref_dB -= spectrumLevelFactor;
                maxLevel -= spectrumLevelFactor;
            }
        }

        void ApplyFilter()
        {
            int n = token.Length;
            float[] fin = new float[n];
            float[] fout = new float[n];
            
            float df = samplingRate_Hz / n;
            float[] w = ComputeRippleWeights(df, n / 2);

            if (precompensate && _level.SPLBasedUnits)
            {
                float[] freq = new float[n / 2];
                for (int k = 0; k < freq.Length; k++) freq[k] = k * df;
                _level.Cal.GetAmplitudesInterp(freq, w);
            }

            alglib.complex[] z = new alglib.complex[n];
            for (int k=0; k<n; k++)
            {
                z[k] = new alglib.complex(token[k]);
            }

            alglib.fftc1d(ref z);

            int forwardIndex = 0;
            int backwardIndex = z.Length - 1;
            for (int k=0; k<w.Length; k++)
            {
                z[forwardIndex++] *= w[k];
                z[backwardIndex--] *= w[k];
            }

            alglib.fftc1dinv(ref z);

            for (int k=0; k<n; k++)
            {
                token[k] = (float) z[k].x;
            }

            float scaleFactor = noiseRMS / KMath.RMS(token);

            for (int k = 0; k < token.Length; k++) token[k] *= scaleFactor;
        }

        private float[] ComputeRippleWeights(float df, int npts)
        {
            float[] w = new float[npts];

            float[] X = new float[npts];

            float f = 0;
            for (int k = 0; k < npts; k++)
            {
                X[k] = KMath.Log2(f / Fmin);

                if (f >= Fmin && f<= Fmax)
                {
                    float oct = KMath.Log2(f / Fmin);
                    float A = Mathf.Sin(2 * Mathf.PI * (Density * oct + Phase));

                    w[k] = Mathf.Pow(10f, 0.5f*Contrast/20*A);
                }

                f += df;
            }

            //Rapid.Tools.Graph.PlotMultiXLim("Ripple Noise", X, -1, KMath.Log2(Fmax / Fmin) + 1, w, "Weights");

            return w;
        }

        override public References Create(float[] data)
        {
            for (int k = 0; k < Npts; k++)
            {
                data[k] = token[curIndex];
				if (++curIndex == token.Length) curIndex = 0;
            }

            return new References(ref_dB, maxLevel);
        }
    }
}
