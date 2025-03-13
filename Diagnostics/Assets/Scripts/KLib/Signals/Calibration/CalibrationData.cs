using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KLib;
using KLib.Signals.Enumerations;

namespace KLib.Signals.Calibration
{
    public class CalibrationData
    {
        public string name;
        float reference;
        float df_Hz;
        float[] dBref_per_1Vpeak;
        float[] V_of_ref;
        float[] dBMax_per_1Vpeak;
        float[] phase_cycles;
        float[] dyn_range;
        float[] spl_of_threshold;
        LevelUnits type;

        public CalibrationData()
        {
            name = "Unnamed";
        }

        public CalibrationData(AcousticCalibration acal)
        {
            name = "Unnamed";
            type = LevelUnits.dB_SPL;
            reference = 100; // dB SPL
            df_Hz = acal.df_Hz;
            dBref_per_1Vpeak = new float[acal.dBSPL_Vrms.Length];
            V_of_ref = new float[acal.dBSPL_Vrms.Length];
            phase_cycles = new float[acal.dBSPL_Vrms.Length];

            dBMax_per_1Vpeak = new float[acal.dBSPL_Vrms.Length];
            for (int k = 0; k < dBMax_per_1Vpeak.Length; k++) dBMax_per_1Vpeak[k] = float.PositiveInfinity;

            // Compute voltage *amplitude*, *per component*, required to produce 100 dB SPL
            for (int k = 0; k < acal.dBSPL_Vrms.Length; k++)
            {
                dBref_per_1Vpeak[k] = acal.dBSPL_Vrms[k] - 20 * Mathf.Log10(Mathf.Sqrt(2));
                dBMax_per_1Vpeak[k] = dBref_per_1Vpeak[k];
                // Intuition:
                // If cal(f) == 100 dB SPL/Vrms, then the rhs=0dB==1
                // If cal(f) < 100 dB SPL/Vrms, then the rhs>0dB>1 ==> more oomph required to get there
                // If cal(f) > 100 dB SPL/Vrms, then the rhs<0dB<1 ==> attenuation required to get down to 100 dB SPL
                V_of_ref[k] = Mathf.Sqrt(2) * Mathf.Pow(10, (reference - acal.dBSPL_Vrms[k]) / 20);
            }
            V_of_ref[0] = 0;
        }

        public float FreqSpaceHz
        {
            get { return df_Hz; }
        }

        public float[] Frequency
        {
            get
            {
                float[] f = new float[dBref_per_1Vpeak.Length];
                for (int k = 0; k < f.Length; k++) f[k] = k * df_Hz;
                return f;
            }
        }

        public float[] MaxMag
        {
            get { return dBMax_per_1Vpeak; }
        }

        public static CalibrationData Create_dBVrms(AcousticCalibration acal)
        {
            CalibrationData c = new CalibrationData();
            c.type = LevelUnits.dB_Vrms;
            c.reference = 0; // dB Vrms
            c.df_Hz = (acal != null) ? acal.df_Hz : 0; // doesn't matter, will be ignored

            int npts = (acal != null) ? acal.dBSPL_Vrms.Length : 1;

            c.dBref_per_1Vpeak = new float[npts];
            c.V_of_ref = new float[npts];
            c.dBMax_per_1Vpeak = new float[npts];

            for (int k=0; k<npts; k++)
            {
                c.dBref_per_1Vpeak[k] = -20 * Mathf.Log10(Mathf.Sqrt(2));

                // Voltage *amplitude*, *per component*, required to produce 1 Vrms
                c.V_of_ref[k] = Mathf.Sqrt(2);

                c.dBMax_per_1Vpeak[k] = c.dBref_per_1Vpeak[0]; // implicitly, this corresponds to the max SPL
            }

            return c;
        }

        public static CalibrationData Create_dBAtten()
        {
            // This is really a dummy calibration, to avoid complicating the waveform synthesis functions 
            CalibrationData c = new CalibrationData();
            c.type = LevelUnits.dB_attenuation;
            c.reference = 0; // dB atten

            c.df_Hz = 0; // doesn't matter, will be ignored

            int npts = 1;

            c.dBref_per_1Vpeak = new float[npts];
            c.V_of_ref = new float[npts];
            c.dBMax_per_1Vpeak = new float[npts];

            for (int k = 0; k < npts; k++)
            {
                c.dBref_per_1Vpeak[k] = 0;

                // Voltage *amplitude*, *per component*, required to produce 1 Vrms
                c.V_of_ref[k] = 1;

                c.dBMax_per_1Vpeak[k] = c.dBref_per_1Vpeak[0]; // implicitly, this corresponds to the max SPL
            }

            return c;
        }

        public static CalibrationData Create_dBHL(AcousticCalibration acal)
        {
            CalibrationData c = new CalibrationData();
            c.type = LevelUnits.dB_HL;
            c.reference = 75; // dB HL
            c.df_Hz = acal.df_Hz;
            c.dBref_per_1Vpeak = new float[acal.dBSPL_Vrms.Length];
            c.V_of_ref = new float[acal.dBSPL_Vrms.Length];
            c.dBMax_per_1Vpeak = new float[acal.dBSPL_Vrms.Length];

            float[] SPLgivingHLeq0 = Audiograms.ANSI_dBHL.GetTable().Interp1(acal.df_Hz, acal.dBSPL_Vrms.Length);

            // Compute voltage *amplitude*, *per component*, required to produce 75 dB HL
            for (int k = 0; k < acal.dBSPL_Vrms.Length; k++)
            {
                float dBHL_Vrms = acal.dBSPL_Vrms[k] - SPLgivingHLeq0[k];

                c.dBref_per_1Vpeak[k] = dBHL_Vrms - 20 * Mathf.Log10(Mathf.Sqrt(2));
                // Intuition:
                // If cal(f) == 75 dB HL/Vrms, then the rhs=0dB==1
                // If cal(f) < 75 dB HL/Vrms, then the rhs>0dB>1 ==> more oomph required to get there
                // If cal(f) > 75 dB HL/Vrms, then the rhs<0dB<1 ==> attenuation required to get down to 100 dB SPL
                c.V_of_ref[k] = Mathf.Sqrt(2) * Mathf.Pow(10, (c.reference - dBHL_Vrms) / 20);

                c.dBMax_per_1Vpeak[k] = c.dBref_per_1Vpeak[k];
            }
            c.V_of_ref[0] = 0;

            return c;
        }

        public static CalibrationData Create_dBSL(Audiograms.Audiogram audiogram, AcousticCalibration acal)
        {
            CalibrationData c = new CalibrationData();
            c.type = LevelUnits.dB_SL;
            c.reference = 75; // dB SL
            c.df_Hz = acal.df_Hz;
            c.dBref_per_1Vpeak = new float[acal.dBSPL_Vrms.Length];
            c.dBMax_per_1Vpeak = new float[acal.dBSPL_Vrms.Length];
            c.V_of_ref = new float[acal.dBSPL_Vrms.Length];

            List<float> validFreq = new List<float>();
            List<float> validThresh = new List<float>();
            for (int k = 0; k < audiogram.Threshold_dBSPL.Length; k++)
            {
                if (!float.IsNaN(audiogram.Threshold_dBSPL[k]) && !float.IsInfinity(audiogram.Threshold_dBSPL[k]))
                {
                    validFreq.Add(audiogram.Frequency_Hz[k]);
                    validThresh.Add(audiogram.Threshold_dBSPL[k]);
                }
            }

            float[] SPLgivingSLeq0 = KMath.Interp1(validFreq.ToArray(), validThresh.ToArray(), acal.df_Hz, acal.dBSPL_Vrms.Length);

            // Compute voltage *amplitude*, *per component*, required to produce 75 dB SL
            for (int k = 0; k < acal.dBSPL_Vrms.Length; k++)
            {
                float dBSL_Vrms = acal.dBSPL_Vrms[k] - SPLgivingSLeq0[k];

                c.dBref_per_1Vpeak[k] = dBSL_Vrms - 20 * Mathf.Log10(Mathf.Sqrt(2));
                // Intuition:
                // If cal(f) == 75 dB HL/Vrms, then the rhs=0dB==1
                // If cal(f) < 75 dB HL/Vrms, then the rhs>0dB>1 ==> more oomph required to get there
                // If cal(f) > 75 dB HL/Vrms, then the rhs<0dB<1 ==> attenuation required to get down to 75 dB SL
                c.V_of_ref[k] = Mathf.Sqrt(2) * Mathf.Pow(10, (c.reference - dBSL_Vrms) / 20);

                // max SPL - thresh SPL = max SL
                c.dBMax_per_1Vpeak[k] = acal.dBSPL_Vrms[k] - 20 * Mathf.Log10(Mathf.Sqrt(2)) - SPLgivingSLeq0[k];
            }
            c.V_of_ref[0] = 0;

            return c;
        }

        public void ComputeDynamicRange(Audiograms.Audiogram agram, Audiograms.Audiogram ldlgram)
        {
            List<float> validFreq = new List<float>();
            List<float> validThresh = new List<float>();
            for (int k = 0; k < agram.Threshold_dBSPL.Length; k++)
            {
                if (!float.IsNaN(agram.Threshold_dBSPL[k]) && !float.IsInfinity(agram.Threshold_dBSPL[k]))
                {
                    validFreq.Add(agram.Frequency_Hz[k]);
                    validThresh.Add(agram.Threshold_dBSPL[k]);
                }
            }

            spl_of_threshold = KMath.Interp1(validFreq.ToArray(), validThresh.ToArray(), df_Hz, dBMax_per_1Vpeak.Length);

            validFreq.Clear();
            List<float> validLimit = new List<float>();
            for (int k = 0; k < ldlgram.Threshold_dBSPL.Length; k++)
            {
                if (!float.IsNaN(ldlgram.Threshold_dBSPL[k]))
                {
                    validFreq.Add(ldlgram.Frequency_Hz[k]);
                    validLimit.Add(ldlgram.Threshold_dBSPL[k]);
                }
            }

            float[] ldl = KMath.Interp1(validFreq.ToArray(), validLimit.ToArray(), df_Hz, dBMax_per_1Vpeak.Length);

            dyn_range = new float[ldl.Length];
            for (int k=0; k<ldl.Length; k++)
            {
                dyn_range[k] = ldl[k] - spl_of_threshold[k];
            }
            type = LevelUnits.PercentDR;
        }

        public void SetUpperBounds(AcousticCalibration acal, Audiograms.Audiogram ldlgram, float maxLevelMargin)
        {
            List<float> validFreq = new List<float>();
            List<float> validLimit = new List<float>();

            for (int k = 0; k < ldlgram.Threshold_dBSPL.Length; k++)
            {
                if (!float.IsNaN(ldlgram.Threshold_dBSPL[k]))
                {
                    validFreq.Add(ldlgram.Frequency_Hz[k]);
                    validLimit.Add(ldlgram.Threshold_dBSPL[k] - maxLevelMargin);
                }
            }

            if (validFreq.Count > 0)
            {
                float[] interpLDL = KMath.Interp1(validFreq.ToArray(), validLimit.ToArray(), df_Hz, dBMax_per_1Vpeak.Length);

                for (int k = 0; k < dBMax_per_1Vpeak.Length; k++)
                {
                    // -- to this point, the max has already been set in terms of SPL.
                    // -- compute the additional attenuation required by the LDL limit
                    float atten = Mathf.Min(0, interpLDL[k] - (acal.dBSPL_Vrms[k] - KMath.Three_dB));

                    dBMax_per_1Vpeak[k] += atten;
                }
            }
        }

        public LevelUnits Type
        {
            get { return type; }
        }

        public float Reference
        {
            get { return reference; }
        }

        public float GetAmplitude(float freq_Hz)
        {
            int idx = (df_Hz > 0) ? (int)Mathf.Round(freq_Hz / df_Hz) : 0;
            return V_of_ref[idx];
        }

        public float GetMaxAmplitude(float fmin, float fmax)
        {
            float Amax = 0;
            float freq = 0;

            fmin = Mathf.Floor(fmin / df_Hz) * df_Hz;
            fmax = Mathf.Ceil(fmax / df_Hz) * df_Hz;

            for (int k = 0; k < V_of_ref.Length; k++)
            {
                if (freq >= fmin && freq <= fmax && V_of_ref[k] > Amax)
                {
                    Amax = V_of_ref[k];
                }
                freq += df_Hz;
            }
            return Amax;
        }

        public void GetAmplitudesInterp(float[] freq, float[] w)
        {
            float[] A = KMath.Interp1(Frequency, V_of_ref, freq);
            for (int k = 0; k < w.Length; k++) w[k] *= A[k];
        }

        public float[] GetReference()
        {
            return dBref_per_1Vpeak.Clone() as float[];
        }

        public float GetReference(float freq_Hz)
        {
            int idx = (df_Hz > 0) ? (int)Mathf.Round(freq_Hz / df_Hz) : 0;
            return dBref_per_1Vpeak[idx];
        }

        public References GetAllReferences(float freq_Hz)
        {
            References r = new References();
            int idx = (df_Hz > 0) ? (int)Mathf.Round(freq_Hz / df_Hz) : 0;
            r.refVal = dBref_per_1Vpeak[idx];
            r.maxVal = dBMax_per_1Vpeak[idx];

            if (dyn_range != null)
            {
                r.dynamicRange = dyn_range[idx];
                r.thrSPL = spl_of_threshold[idx];
            }

            return r;
        }

        public float GetReference(float fmin, float fmax)
        {
            int k1 = (df_Hz > 0) ? (int)Mathf.Round(fmin / df_Hz) : 0;
            int k2 = (df_Hz > 0) ? (int)Mathf.Round(fmax / df_Hz) : 0;

            float s = 0;
            float n = 0;
            for (int k = k1; k <= k2; k++)
            {
                s += Mathf.Pow(10f, dBref_per_1Vpeak[k] / 10);
                ++n;
            }

            return 10f * Mathf.Log10(s / n);
        }

        public float GetWeightedReference(float[] w, float Fs)
        {
            return WeightedSum(dBref_per_1Vpeak, w, Fs);
        }

        public float GetWeightedMax(float[] w, float Fs)
        {
            return WeightedSum(dBMax_per_1Vpeak, w, Fs);
        }

        public float GetReference(float[] Y, float Fs)
        {
            return SpectrumWeightedSum(dBref_per_1Vpeak, Y, Fs);
        }

        public float GetMax(float[] Y, float Fs)
        {
            return SpectrumWeightedSum(dBMax_per_1Vpeak, Y, Fs);
        }

        private float WeightedSum(float[] calData, float[] w, float Fs)
        {
            float[] cal_i = KMath.Interp1(Frequency, calData, 0.5f*Fs/w.Length, w.Length);

            float ss = 0;
            for (int k = 0; k < w.Length; k++)
            {
                ss += 2 * Mathf.Pow(10f, cal_i[k] / 10) * w[k] * w[k];
            }

            return 10f * Mathf.Log10(ss);
        }

        private float SpectrumWeightedSum(float[] calData, float[] Y, float Fs)
        {
            int n = Y.Length;
            alglib.complex[] z = new alglib.complex[n];
            for (int k = 0; k < n; k++)
            {
                z[k] = new alglib.complex(Y[k]);
            }
            alglib.fftc1d(ref z);


            float[] w = new float[n / 2];
            for (int k = 0; k < n / 2; k++) w[k] = Mathf.Sqrt(2) * Magnitude(z[k]/n);

            return WeightedSum(calData, w, Fs);
        }


        private float Magnitude(alglib.complex z)
        {
            return Mathf.Sqrt((float)(z.x * z.x + z.y * z.y));
        }

        public float[] GetMax()
        {
            return dBMax_per_1Vpeak.Clone() as float[];
        }

        public float GetMax(float freq_Hz)
        {
            int idx = (df_Hz > 0) ? (int)Mathf.Round(freq_Hz / df_Hz) : 0;
            return dBMax_per_1Vpeak[idx];
        }

        public float GetMax(float fmin, float fmax)
        {
            int k1 = (df_Hz > 0) ? (int)Mathf.Round(fmin / df_Hz) : 0;
            int k2 = (df_Hz > 0) ? (int)Mathf.Round(fmax / df_Hz) : 0;

            float s = 0;
            float n = 0;
            for (int k = k1; k <= k2; k++)
            {
                s += Mathf.Pow(10f, dBMax_per_1Vpeak[k] / 10);
                ++n;
            }

            return 10f * Mathf.Log10(s / n);
        }

    }
}
