using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using KLib.Signals;

namespace SpeechReception
{
    public class Masker
    {
        //[PropertyOrder(0)]
        public string Source { get; set; }
        private bool ShouldSerializeSource() { return false; }

        public int NumBabblers { get; set; }
        public bool ShouldSerializeNumBabblers() { return Source.Equals("IEEE"); }

        public int BabbleSeed { get; set; }
        public bool ShouldSerializeBabbleSeed() { return Source.Equals("IEEE"); }

        public Masker()
        {
            Source = "None";
            NumBabblers = 4;
            BabbleSeed = 0;
        }
    }
}
