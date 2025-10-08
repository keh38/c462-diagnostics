using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using KLib;
using KLib.Signals;
using KLib.Signals.Waveforms;

using Newtonsoft.Json;
using ProtoBuf;

namespace Audiograms
{
    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class AudiogramData
    {
        public List<Audiogram> audiograms = new List<Audiogram>();
        public AudiogramData() { }

        public void Initialize(float[] Frequency_Hz)
        {
            audiograms.Add(new Audiogram(Ear.Left));
            audiograms.Add(new Audiogram(Ear.Right));

            audiograms.Find(a => a.ear == Ear.Left).Initialize(Frequency_Hz);
            audiograms.Find(a => a.ear == Ear.Right).Initialize(Frequency_Hz);
        }

        public void Append(float[] Frequency_Hz)
        {
            audiograms.Find(a => a.ear == Ear.Left).Append(Frequency_Hz);
            audiograms.Find(a => a.ear == Ear.Right).Append(Frequency_Hz);
        }

        public void Clear(float[] Frequency_Hz)
        {
            audiograms.Find(a => a.ear == Ear.Left).Initialize(Frequency_Hz);
            audiograms.Find(a => a.ear == Ear.Right).Initialize(Frequency_Hz);
        }

        public void Set(KLib.Signals.Laterality laterality, float Frequency_Hz, float ThresholdHL, float ThresholdSPL)
        {
            var ear = (laterality == Laterality.Left) ? Ear.Left : Ear.Right;
            Set(ear, Frequency_Hz, ThresholdHL, ThresholdSPL);
        }

        //public void Set(KLib.Signals.Laterality laterality, string stimulusType, float ThresholdSPL)
        //{
        //    var ear = (laterality == Laterality.Left) ? Ear.Left : Ear.Right;
        //    Set(ear, stimulusType, ThresholdSPL);
        //}

        //public void Set(Ear ear, string stimulusType, float ThresholdSPL)
        //{
        //    audiograms.Find(a => a.ear == ear).Set(stimulusType, ThresholdSPL);
        //}

        public void Set(Ear ear, float Frequency_Hz, float ThresholdHL, float ThresholdSPL)
        {
            audiograms.Find(a => a.ear == ear).Set(Frequency_Hz, ThresholdHL, ThresholdSPL);
        }

        public void Insert(Ear ear, float Frequency_Hz, float ThresholdSPL)
        {
            audiograms.Find(o => o.ear == ear).Insert(Frequency_Hz, ThresholdSPL);
        }

        public Audiogram Get(Ear ear)
        {
            return audiograms.Find(a => a.ear == ear);
        }

        public Audiogram Get(string destination)
        {
            if (destination.ToLower().Equals("mono"))
            {
                return MeanAudiogram();
            }

            return audiograms.Find(a => a.ear.ToString().ToLower() == destination.ToLower());
        }

        public AudiogramData ReplaceNaNWithMax(string transducer)
        {
            var cal = KLib.Signals.Calibration.CalibrationFactory.Load(LevelUnits.dB_SPL_noLDL, transducer, "");

            foreach (var a in audiograms)
            {
                for (int k = 0; k < a.Frequency_Hz.Length; k++)
                {
                    //Debug.Log(a.Frequency_Hz[k]);
                    if (float.IsNaN(a.Threshold_dBSPL[k]))
                    {
                        a.Threshold_dBSPL[k] = cal.GetMax(a.Frequency_Hz[k]);
                        //Debug.Log("Replaced " + a.Frequency_Hz[k] + " Hz with " + a.Threshold_dBSPL[k]);
                    }
                }
            }
            return this;
        }

        private Audiogram MeanAudiogram()
        {
            Audiogram mean = new Audiogram();
            mean.Initialize(audiograms[0].Frequency_Hz);
            foreach (var a in audiograms.FindAll(o => o.Frequency_Hz.Length > 0))
            {
                mean.Append(a.Frequency_Hz);
            }

            float[] hl = new float[mean.Frequency_Hz.Length];
            float[] spl = new float[mean.Frequency_Hz.Length];

            int n = 0;
            foreach (var a in audiograms.FindAll(o => o.Frequency_Hz.Length > 0))
            {
                n++;
                for (int k = 0; k < mean.Frequency_Hz.Length; k++)
                {
                    hl[k] += a.GetHL(mean.Frequency_Hz[k]);
                    spl[k] += a.GetThreshold(mean.Frequency_Hz[k]);
                }
            }

            for (int k = 0; k < mean.Frequency_Hz.Length; k++)
            {
                mean.Set(mean.Frequency_Hz[k], hl[k] / n, spl[k] / n);
            }

            return mean;
        }

        public float[] Get_Frequency_Hz()
        {
            List<float> freq = new List<float>();

            foreach (Ear e in new Ear[] { Ear.Left, Ear.Right })
            {
                foreach (float fnew in Get(e).Frequency_Hz)
                {
                    if (freq.Find(f => f == fnew) == 0)
                    {
                        int idx = freq.FindLastIndex(f => f < fnew);
                        freq.Insert(idx + 1, fnew);
                    }
                }
            }
            return freq.ToArray();
        }

        public override string ToString()
        {
            string output = "Freq(Hz), Left (dB SPL), Right (dB SPL)\n";

            Audiogram left = Get(Ear.Left);
            Audiogram right = Get(Ear.Right);

            for (int k=0; k<audiograms[0].Frequency_Hz.Length; k++)
            {
                output += ((int)left.Frequency_Hz[k]).ToString("D2") + ", " + 
                        left.Threshold_dBSPL[k].ToString("F1") + ", " +
                        right.Threshold_dBSPL[k].ToString("F1");
            }

            return output;
        }

        public void Save()
        {
            Save(FileLocations.AudiogramPath);
        }

        public void Save(string path)
        {
            FileIO.XmlSerialize(this, path);
        }

        public static AudiogramData Load()
        {
            return Load(FileLocations.AudiogramPath);
        }

        public float MaxAudibleFrequency(float nominalMax)
        {
            float fmax = float.NegativeInfinity;
            foreach (Audiogram a in audiograms)
            {
                fmax = Mathf.Max(fmax, a.GetMaxAudibleFrequency(nominalMax));
            }

            return fmax;
        }

        public static AudiogramData Load(string path)
        {
            if (File.Exists(path))
            {
                return FileIO.XmlDeserialize<AudiogramData>(path);
            }
            return null;
        }
    }
}