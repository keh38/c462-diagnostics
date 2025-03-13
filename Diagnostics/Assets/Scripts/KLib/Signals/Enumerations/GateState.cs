using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KLib.Signals.Enumerations
{
    public enum GateState 
    { 
        Idle,
        Delay,
        RampUp,
        On,
        RampDown,
        Off,
        Finished
    };
}
