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
   [System.Serializable]
    public class ANSI_dBHL
    {
        public float[] Freq_Hz;
        public float[] dBSPL;

        public static ANSI_dBHL GetTable()
        {
            return FileIO.XmlDeserializeFromTextAsset<ANSI_dBHL>("ANSI_dBHL");
        }

        public static ANSI_dBHL GetTable(string transducer)
        {
            return FileIO.XmlDeserializeFromTextAsset<ANSI_dBHL>($"ANSI_dBHL_{transducer}");
        }

        public float HL_To_SPL(float freq)
        {
            if (freq == 0)
            {
                return 0;
            }

            return KMath.Interp1(Freq_Hz, dBSPL, freq);
//            int idx = Array.IndexOf(Freq_Hz, freq);
//            return dBSPL[idx];
        }

        public float SPL_To_HL(float freq, float SPL)
        {
            if (freq == 0)
            {
                return SPL;
            }

            float spl_of_hl_eq_0 = 0;;
            int idx = Array.IndexOf(Freq_Hz, freq);
            if (idx >= 0)
            {
                spl_of_hl_eq_0 = dBSPL[idx];
            }
            else
            {
                spl_of_hl_eq_0 = KMath.Interp1(Freq_Hz, dBSPL, freq);
            }

            return SPL - spl_of_hl_eq_0;
        }

        public float[] Interp1(float df, int npts)
        {
            return KMath.Interp1(Freq_Hz, dBSPL, df, npts);
        }
    }
}