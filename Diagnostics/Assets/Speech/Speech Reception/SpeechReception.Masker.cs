using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using KLib.Signals.Enumerations;

namespace SpeechReception
{
    [System.Serializable]
    public class Masker
    {
        public string Source = "None";
        public int NumBabblers = 4;
        public int BabbleSeed = 0;
    }
}
