using System;
using System.Collections;
using System.Collections.Generic;

using KLib.Signals.Enumerations;

namespace SpeechReception
{
    [System.Serializable]
    public class Calibration
    {
        public string transducer = "Bose";
        public float dB_SPL = float.NaN;
        public float dB_HL = float.NaN;
        public float dB_Vrms = float.NaN;
    }
}