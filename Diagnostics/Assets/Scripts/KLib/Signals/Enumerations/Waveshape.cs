using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KLib.Signals.Enumerations
{
    public enum Waveshape
    { 
        None, 
        Sinusoid, 
        Noise, 
        MovingRippleNoise, 
        ToneCloud, 
        FM,
        File,
        RippleNoise
    }
}
