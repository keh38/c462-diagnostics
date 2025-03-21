using UnityEngine;
using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals.Calibration;
using KLib.Signals.Enumerations;
using KLib.Signals.Filters;

namespace KLib.Signals.Waveforms
{
	[System.Serializable]
	[ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
	[JsonObject(MemberSerialization.OptOut)]
	public class Noise : Waveform
    {
        [ProtoMember(1, IsRequired = true)]
        public FilterSpec filter = new FilterSpec();
        [ProtoMember(2, IsRequired = true)]
        public bool precompensate;
        [ProtoMember(3, IsRequired = true)]
        public int seed;

		int _curIndex;

		float[] _token = null;
        private static readonly float tokenLength_s = 2.631f;
        private float _normRMS = 0.25f;
        private float _unifRMS;

        private float _ref_dB;
        private float _maxLevel;

        private float[] _cf;
        private float[] _ref_vs_cf;
        private float[] _max_vs_cf;
            
        private bool _sweepableFilter = false;
        private bool _uniform = false;

        private BPFilter _bpFilter;

        private int _sharedSeed = -1;

        public Noise()
        {
			shape = Waveshape.Noise;
            _shortName = "Noise";

            _unifRMS = 1 / Mathf.Sqrt(3);
        }

        public override float GetMaxLevel(Level level, float Fs)
        {
            ComputeReferences(level, Fs, out _ref_dB, out _maxLevel);
            return _maxLevel;
        }

        public override List<string> GetSweepableParams()
        {
            return new List<string>()
            {
                "CF"
            };
        }

        [ProtoIgnore]
        [JsonIgnore]
        public float[] FilterCF
        {
            get { return _cf; }
        }

        [ProtoIgnore]
        [JsonIgnore]
        public float[] FilterMaxLevel
        {
            get { return _max_vs_cf; }
        }

        override public Action<float> GetParamSetter(string paramName)
        {
            Action<float> setter = null;
            switch (paramName)
            {
                case "CF":
                    setter = x => 
                    {
                        SetCF(x);
                        _bpFilter.SetProperties(filter.CF, filter.GetLinearBW());
                        UpdateSweptReferences(filter.CF);
                    };
                    break;
            }

            return setter;
        }

        override public List<string> GetValidParameters()
        {
            List<string> plist = new List<string>();
            plist.Add("CF_Hz");
            plist.Add("BW");
            plist.Add("Shape");
            return plist;
        }

        override public string SetParameter(string paramName, float value)
        {
            switch (paramName)
            {
                case "CF_Hz":
                    SetCF(value);
                    //                    _bpFilter.SetProperties(filter.CF, filter.GetLinearBW());
                    ComputeReferences(_channel.level, samplingRate_Hz, out _ref_dB, out _maxLevel);
                    break;

                case "BW":
                    SetBW(value);
                    ComputeReferences(_channel.level, samplingRate_Hz, out _ref_dB, out _maxLevel);
                    break;

                case "Shape":
                    SetShape((FilterShape) value);
                    ComputeReferences(_channel.level, samplingRate_Hz, out _ref_dB, out _maxLevel);
                    break;
            }

            return "";
        }

        public void SetSharedSeed(int value)
        {
            _sharedSeed = value;
        }

        override public void Initialize(float Fs, int N, Channel channel)
        {
			base.Initialize(Fs, N, channel);

            _sweepableFilter = filter.shape != FilterShape.None && filter.sweepable;
            _uniform = filter.shape != FilterShape.None && !filter.brickwall && !filter.unityFilter;

            if (filter.shape != FilterShape.None && !filter.brickwall && !filter.unityFilter)
            {
                _bpFilter = new BPFilter(2, filter.CF, filter.GetLinearBW(), samplingRate_Hz);
            }

            _curIndex = 0;
            CreateToken(_channel.gate.NMax);

            if (_sweepableFilter)
            {
                ComputeSweepableReferences();
                UpdateSweptReferences(filter.CF);
            }
            else ComputeReferences(_channel.level, samplingRate_Hz, out _ref_dB, out _maxLevel);
        }

        public void SetCF(float cf)
        {
            filter.Set(filter.bandwidthMethod, cf, filter.BW);
        }

        public void SetBW(float bw)
        {
            filter.Set(filter.bandwidthMethod, filter.CF, bw);
        }

        public void SetShape(FilterShape shape)
        {
            filter.shape = shape;
        }

        public void SetFilter(float cf, float bw)
        {
            filter.Set(filter.bandwidthMethod, cf, bw);
            ComputeReferences(_channel.level, samplingRate_Hz, out _ref_dB, out _maxLevel);
        }

        private void CreateToken(int nmax)
        {
            //Debug.Log(ChannelName + " seed=" + (_sharedSeed > 0 ? _sharedSeed : seed));
            var normrnd = new KLib.Math.GaussianRandom(_sharedSeed > 0 ? _sharedSeed : seed);
            var unifrnd = new System.Random();

            int tokenLength = nmax;
            if (nmax < 0) tokenLength = Mathf.Max(Npts, Mathf.RoundToInt(tokenLength_s * samplingRate_Hz));

#if KDEBUG
            token = new float[tokenLength];
            for (int k = 0; k < tokenLength; k++) token[k] = 0;
            return;
#endif

            if (_uniform)
            {
                if (_token == null)
                {
                    _token = new float[tokenLength];
//                    for (int k = 0; k < tokenLength; k++) token[k] = (2 * UnityEngine.Random.Range(-1, 1) - 1);
                    for (int k = 0; k < tokenLength; k++) _token[k] = (2 * (float)unifrnd.NextDouble() - 1);
                }
            }
            else
            {
                _token = new float[tokenLength];
                for (int k = 0; k < tokenLength; k++) _token[k] = Mathf.Clamp(normrnd.Next(0, _normRMS), -1, 1);
            }

            if (filter.brickwall && (filter.shape != FilterShape.None || (precompensate && (_channel.level.Units == LevelUnits.dB_SPL || _channel.level.Units == LevelUnits.dB_SPL_noLDL))))
            {
                ApplyFilter();
            }
        }

        private void ComputeReferences(Level level, float Fs, out float ref_dB_out, out float maxLevel_out)
        {
            float ref_dB = _uniform ? 20 * Mathf.Log10(_unifRMS) : 20 * Mathf.Log10(_normRMS);
            float maxLevel = ref_dB;

            float stimRMS = _uniform ? _unifRMS : _normRMS;

            float bwFactor = filter.ComputeBandwidthFactor(Fs);

            if (level.Units == LevelUnits.dB_SPL || level.Units == LevelUnits.dB_HL || level.Units == LevelUnits.dB_SL || level.Units == LevelUnits.dB_SPL_noLDL)
            {
                if (level.Cal == null)
                    throw new ApplicationException("Null calibration.");

                int n = Mathf.RoundToInt(tokenLength_s * Fs);
                float df = Fs / (float)n;
                float[] w = filter.GetWeights(df, n / 2);

                float[] freq = new float[n / 2];

                float scale = Mathf.Sqrt(1.0f / n) * stimRMS / Mathf.Sqrt(bwFactor);

                for (int k = 0; k < w.Length; k++)
                {
                    freq[k] = k * df;
                    w[k] *= scale;
                }

                if (precompensate && (level.Units == LevelUnits.dB_SPL || level.Units == LevelUnits.dB_SPL_noLDL))
                {
                    level.Cal.GetAmplitudesInterp(freq, w);
                }
                ref_dB = level.Cal.GetWeightedReference(w, Fs);
                maxLevel = level.Cal.GetWeightedMax(w, Fs);
            }

            if (level.Reference == LevelReference.Spectrum_level)
            {
                float spectrumLevelFactor = 10 * Mathf.Log10(Fs / 2) + 10 * Mathf.Log10(bwFactor);
                ref_dB -= spectrumLevelFactor;
                maxLevel -= spectrumLevelFactor;
            }

            if (filter.shape != FilterShape.None && !filter.brickwall && !filter.unityFilter)
            {
                ref_dB += 20 * Mathf.Log10(Mathf.Sqrt(bwFactor));
                maxLevel += 20 * Mathf.Log10(Mathf.Sqrt(bwFactor));
            }

            ref_dB_out = ref_dB;
            maxLevel_out = maxLevel;

            //Debug.Log(filter.ToString() + ": Ref = " + ref_dB.ToString("F1") + ", Max = " + maxLevel.ToString("F1"));
        }

        void ApplyFilter()
        {
            int n = _token.Length;
            float[] fin = new float[n];
            float[] fout = new float[n];
            
            float df = samplingRate_Hz / n;
            float[] w = filter.GetWeights(df, n / 2);

            if (precompensate && _channel.level.Units == LevelUnits.dB_SPL)
            {
                float[] freq = new float[n / 2];
                for (int k = 0; k < freq.Length; k++) freq[k] = k * df;
                _channel.level.Cal.GetAmplitudesInterp(freq, w);
            }

            alglib.complex[] z = new alglib.complex[n];
            for (int k=0; k<n; k++)
            {
                z[k] = new alglib.complex(_token[k]);
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
                _token[k] = (float) z[k].x;
            }

            float scaleFactor = _normRMS / KMath.RMS(_token);

            for (int k = 0; k < _token.Length; k++) _token[k] *= scaleFactor;
        }

        private void ComputeSweepableReferences()
        {
            float fmin = 250;
            float fmax = 16000;
            float fstep_oct = 0.25f;

            int nf = Mathf.RoundToInt(Mathf.Log(fmax / fmin) / Mathf.Log(2) / fstep_oct) + 1;

            _cf = new float[nf];
            _ref_vs_cf = new float[nf];
            _max_vs_cf = new float[nf];

            var original_cf = filter.CF;

            for (int k=0; k < nf; k++)
            {
                _cf[k] = fmin * Mathf.Pow(2, k * fstep_oct);

                filter.Set(filter.bandwidthMethod, _cf[k], filter.BW);
                filter.CF = _cf[k];
                ComputeReferences(_channel.level, samplingRate_Hz, out _ref_vs_cf[k], out _max_vs_cf[k]);
            }

            filter.Set(filter.bandwidthMethod, original_cf, filter.BW);
        }

        private void UpdateSweptReferences(float cf)
        {
            _ref_dB = KMath.Interp1(_cf, _ref_vs_cf, cf);
            _maxLevel = KMath.Interp1(_cf, _max_vs_cf, cf);
        }

        override public References Create(float[] data)
        {
            if (filter.shape != FilterShape.None && !filter.brickwall && !filter.unityFilter)
            {
                for (int k = 0; k < Npts; k++)
                {
                    data[k] = _bpFilter.Filter(_token[_curIndex]);
                    if (++_curIndex == _token.Length) _curIndex = 0;
                }
            }
            else
            {
                //Debug.Log($"{ChannelName}: {_curIndex} {_token[_curIndex]}");
                for (int k = 0; k < Npts; k++)
                {
                    data[k] = _token[_curIndex];
                    if (++_curIndex == _token.Length) _curIndex = 0;
                }
            }

            //            Debug.Log("create(" + _channel.level.Units + "): Ref = " + _ref_dB.ToString("F1") + ", Max = " + _maxLevel.ToString("F1"));
            return new References(_ref_dB, _maxLevel);
        }
    }
}
