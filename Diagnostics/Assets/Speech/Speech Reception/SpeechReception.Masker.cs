using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

using KLib.Signals.Enumerations;
using OrderedPropertyGrid;

namespace SpeechReception
{
    [TypeConverter(typeof(MaskerConverter))]
    public class Masker
    {
        //[PropertyOrder(0)]
        [Browsable(false)]
        public string Source { get; set; }
        private bool ShouldSerializeSource() { return false; }

        [PropertyOrder(1)]
        public int NumBabblers { get; set; }
        private bool ShouldSerializeNumBabblers() { return false; }

        [PropertyOrder(2)]
        public int BabbleSeed { get; set; }
        private bool ShouldSerializeBabbleSeed() { return false; }

        public Masker()
        {
            Source = "None";
            NumBabblers = 4;
            BabbleSeed = 0;
        }
    }
}
