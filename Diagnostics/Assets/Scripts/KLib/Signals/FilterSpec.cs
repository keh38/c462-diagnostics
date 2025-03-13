using UnityEngine;
using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals.Enumerations;

namespace KLib.Signals
{
    [System.Serializable]
    [JsonObject(MemberSerialization.OptOut)]
    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    public class FilterSpec
    {
        [ProtoMember(1, IsRequired = true)]
        public FilterShape shape = FilterShape.None;

        [ProtoMember(2, IsRequired = true)]
        public float CF = 500;
        [ProtoMember(3, IsRequired = true)]
        public float BW = 1;
        [ProtoMember(4, IsRequired = true)]
        public float Fmin = 450;
        [ProtoMember(5, IsRequired = true)]
        public float Fmax = 550;
        [ProtoMember(6, IsRequired = true)]
        public BandwidthMethod bandwidthMethod = BandwidthMethod.Edges;
        [ProtoMember(7, IsRequired = true)]
        public bool unityFilter = false;
        [ProtoMember(8, IsRequired = true)]
        public bool sweepable = false;
        [ProtoMember(9, IsRequired = true)]
        public bool brickwall = false;

        public FilterSpec()
        {
        }

        public FilterSpec(BandwidthMethod type, float arg1, float arg2)
        {
            Set(type, arg1, arg2);
        }

        public FilterSpec(FilterShape shape, BandwidthMethod type, float arg1, float arg2)
        {
            this.shape = shape;
            Set(type, arg1, arg2);
        }

        public void Set(BandwidthMethod type, float arg1, float arg2)
        {
            if (shape == FilterShape.High_pass || shape == FilterShape.Low_pass)
            {
                CF = arg1;
                return;
            }

            bandwidthMethod = type;
            switch (type)
            {
                case BandwidthMethod.Edges:
                    Fmin = arg1;
                    Fmax = arg2;
                    break;
                case BandwidthMethod.Hz:
                    CF = arg1;
                    BW = arg2;
                    Fmin = CF - BW / 2;
                    Fmax = CF + BW / 2;
                    break;
                case BandwidthMethod.Octaves:
                    CF = arg1;
                    BW = arg2;
                    Fmin = CF * Mathf.Pow(2, -BW / 2);
                    Fmax = CF * Mathf.Pow(2, BW / 2);
                    break;
                case BandwidthMethod.ERB:
                    CF = arg1;
                    BW = arg2;
                    float erb = ERB(CF);
                    Fmin = CF - BW * erb / 2;
                    Fmax = CF + BW * erb / 2;
                    break;
            }
        }
        
        public void ChangeBandwidthMethod(BandwidthMethod newMethod)
        {
            bandwidthMethod = newMethod;
            switch (bandwidthMethod)
            {
                case BandwidthMethod.Edges:
                    break;
                case BandwidthMethod.Hz:
                    CF = (Fmin + Fmax) / 2;
                    BW = Fmax - Fmin;
                    break;
                case BandwidthMethod.Octaves:
                    CF = (Fmin + Fmax) / 2;
                    BW = Mathf.Log(Fmax / Fmin) / Mathf.Log(2);
                    break;
                case BandwidthMethod.ERB:
                    CF = (Fmin + Fmax) / 2;
                    BW = (Fmax - Fmin) / ERB(CF);
                    break;
            }
        }

        public float GetLinearBW()
        {
            return Fmax - Fmin;
        }

        public static float ERB(float CF)
        {
            return 24.7f * (4.37f * CF/1000f + 1);
        }

        [ProtoIgnore]
        [JsonIgnore]
        public float LPFreq
        {
            get 
            {
                float val = -1;
                switch (bandwidthMethod)
                {
                    case BandwidthMethod.Edges:
                        val = Fmax;
                        break;
                    case BandwidthMethod.Hz:
                        val = CF + BW / 2;
                        break;
                    case BandwidthMethod.Octaves:
                        val = CF * Mathf.Pow(2, BW / 2);
                        break;
                    case BandwidthMethod.ERB:
                        val = CF + ERB(CF) / 2;
                        break;
                }
                return val;
            }
        }

        [ProtoIgnore]
        [JsonIgnore]
        public float HPFreq
        {
            get
            {
                float val = -1;
                switch (bandwidthMethod)
                {
                    case BandwidthMethod.Edges:
                        val = Fmin;
                        break;
                    case BandwidthMethod.Hz:
                        val = CF - BW / 2;
                        break;
                    case BandwidthMethod.Octaves:
                        val = CF * Mathf.Pow(2, -BW / 2);
                        break;
                    case BandwidthMethod.ERB:
                        float erb = 24.7f * (4.37f * CF / 1000f + 1);
                        val = CF - ERB(CF) / 2;
                        break;
                }
                return val;
            }
        }

        public float[] GetWeights(float[] freq)
        {
            float[] w = new float[freq.Length];

            switch (shape)
            {
                case FilterShape.None:
                    for (int k = 0; k < w.Length; k++) w[k] = 1;
                    break;
                case FilterShape.Low_pass:
                    for (int k = 0; k < w.Length; k++) w[k] = (freq[k] <= CF) ? 1 : 0;
                    break;
                case FilterShape.High_pass:
                    for (int k = 0; k < w.Length; k++) w[k] = (freq[k] >= CF) ? 1 : 0;
                    break;
                case FilterShape.Band_pass:
                    for (int k = 0; k < w.Length; k++) w[k] = (freq[k] >= Fmin && freq[k] <= Fmax) ? 1 : 0;
                    break;
                case FilterShape.Band_stop:
                    for (int k = 0; k < w.Length; k++) w[k] = (freq[k] <= Fmin || freq[k] >= Fmax) ? 1 : 0;
                    break;
            }

            return w;
        }

        public float[] GetWeights(float df, int npts)
        {
            float[] freq = new float[npts];
            for (int k = 0; k < npts; k++) freq[k] = k * df;
            return GetWeights(freq);
        }

        public float ComputeBandwidthFactor(float Fs)
        {
            float factor = 1;
            float Fnyq = Fs / 2;

            switch (shape)
            {
                case FilterShape.None:
                    factor = 1;
                    break;
                case FilterShape.Low_pass:
                    factor = CF / Fnyq;
                    break;
                case FilterShape.High_pass:
                    factor = 1 - CF / Fnyq;
                    break;
                case FilterShape.Band_pass:
                    factor = (Fmax - Fmin) / Fnyq;
                    break;
                case FilterShape.Band_stop:
                    factor = 1 - (Fmax - Fmin) / Fnyq;
                    break;
            }
            return factor;
        }

        public override string ToString()
        {
            return "CF = " + CF.ToString("F1") + " Hz; BW = " + BW.ToString("F3") + " " + bandwidthMethod;
        }

    }
}

