using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using KLib.Signals;
using KLib.Signals.Waveforms;

namespace Turandot.Interactive
{
    public class InteractiveControl
    {
        public string channel;
        public string property;
        public string expression;
        public float value;
        public InteractiveControl() { }
    }
}