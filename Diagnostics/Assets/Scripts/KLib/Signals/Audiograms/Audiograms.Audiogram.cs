using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_METRO && !UNITY_EDITOR
using LegacySystem.IO;
#else
using System.IO;
#endif

using KLib;
using KLib.Signals;
using KLib.Signals.Waveforms;

using Newtonsoft.Json;
using ProtoBuf;

namespace Audiograms
{
    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Audiogram
    {
        public Ear ear;
        public float[] Frequency_Hz;
        public float[] Threshold_dBHL;
        public float[] Threshold_dBSPL;
        public SerializeableDictionary<float> StimulusSpecificThresholds = new SerializeableDictionary<float>();

        public Audiogram() { }

        public Audiogram(Ear ear)
        {
            this.ear = ear;
        }

        public void Initialize(float[] Frequency_Hz)
        {
            List<float> freq = new List<float>(Frequency_Hz);
            freq.Sort();
            freq.Remove(0);

            this.Frequency_Hz = (float[])freq.ToArray().Clone();
            Threshold_dBHL = new float[Frequency_Hz.Length];
            Threshold_dBSPL = new float[Frequency_Hz.Length];
            
            for (int k=0; k<Frequency_Hz.Length; k++)
            {
                Threshold_dBHL[k] = Threshold_dBSPL[k] = float.NaN;
            }
        }

        public void Insert(float Frequency_Hz, float ThresholdSPL)
        {
            if (Frequency_Hz == 0)
            {
                StimulusSpecificThresholds["BBN"] = ThresholdSPL;
                return;
            }

            float dBHL = ANSI_dBHL.GetTable().SPL_To_HL(Frequency_Hz, ThresholdSPL);
                
            List<float> freq = new List<float>(this.Frequency_Hz);
            List<float> hl = new List<float>(this.Threshold_dBHL);
            List<float> spl = new List<float>(this.Threshold_dBSPL);

            int idx = freq.IndexOf(Frequency_Hz);
            if (idx < 0)
            {
                idx = freq.FindLastIndex(f => f < Frequency_Hz);
                freq.Insert(idx + 1, Frequency_Hz);
                spl.Insert(idx + 1, ThresholdSPL);
                hl.Insert(idx + 1, dBHL);
            }
            else
            {
                spl[idx] = ThresholdSPL;
                hl[idx] = dBHL;
            }

            if (freq.Find(f => f == Frequency_Hz) == 0)


            this.Frequency_Hz = freq.ToArray();
            this.Threshold_dBHL = hl.ToArray();
            this.Threshold_dBSPL = spl.ToArray();
        }

        public void Append(float[] Frequency_Hz)
        {
            List<float> freq = new List<float>(this.Frequency_Hz);
            List<float> hl = new List<float>(this.Threshold_dBHL);
            List<float> spl = new List<float>(this.Threshold_dBSPL);

            foreach (float fnew in Frequency_Hz)
            {
                if (fnew > 0 && freq.Find(f => f==fnew) == 0)
                {
                    int idx = freq.FindLastIndex(f => f<fnew);
                    freq.Insert(idx+1, fnew);
                    hl.Insert(idx+1, float.NaN);
                    spl.Insert(idx+1, float.NaN);
                }
            }

            this.Frequency_Hz = freq.ToArray();
            this.Threshold_dBHL = hl.ToArray();
            this.Threshold_dBSPL = spl.ToArray();
        }
        
        public float GetMaxAudibleFrequency(float nominalMax)
        {
            float fmax = float.NegativeInfinity;

            for (int k=0; k<Frequency_Hz.Length; k++)
            {
                if (!float.IsNaN(Threshold_dBHL[k]) && Threshold_dBHL[k]<float.PositiveInfinity && Threshold_dBHL[k]>float.NegativeInfinity &&
                    Frequency_Hz[k]>fmax && Frequency_Hz[k]<=nominalMax)
                {
                    fmax = Frequency_Hz[k];
                }
            }
            return fmax;
        }

        public float GetThreshold(float Freq_Hz)
        {
            if (Freq_Hz == 0)
            {
                return StimulusSpecificThresholds["BBN"];
            }
            return KMath.Interp1(Frequency_Hz, Threshold_dBSPL, Freq_Hz);
        }
        
        public float GetHL(float Freq_Hz)
        {
            if (Freq_Hz == 0)
            {
                return StimulusSpecificThresholds["BBN"];
            }
            return KMath.Interp1(Frequency_Hz, Threshold_dBHL, Freq_Hz);
        }
        
        public float GetMeanThreshold(float minFreq, float maxFreq)
        {
            float sum = 0;
            int n = 0;
            for (int k=0; k<Frequency_Hz.Length; k++)
            {
                if (Frequency_Hz[k] >= minFreq && Frequency_Hz[k]<=maxFreq)
                {
                    sum += Threshold_dBSPL[k];
                    ++n;
                }
            }
            return sum / (float)n;
        }

        public void Set(float Frequency_Hz, float ThresholdHL, float ThresholdSPL)
        {
            if (Frequency_Hz == 0)
            {
                StimulusSpecificThresholds["BBN"] = ThresholdSPL;
                return;
            }

            int idx = Array.IndexOf(this.Frequency_Hz, Frequency_Hz);
            Threshold_dBHL[idx] = ThresholdHL;
            Threshold_dBSPL[idx] = ThresholdSPL;
        }

        //public void Set(string stimulusType, float thresholdSPL)
        //{
        //    StimulusSpecificThresholds[stimulusType] = thresholdSPL;
        //}

        public bool IsEmpty()
        {
            bool allEmpty = true;
            foreach(float t in Threshold_dBHL)
            {
                if (!float.IsNaN(t))
                {
                    allEmpty = false;
                }
            }
            return allEmpty;
        }
    }
}