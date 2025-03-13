using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KLib.Signals.Calibration
{
    public class References
    {
        public float refVal;
        public float maxVal;
        public float dynamicRange;
        public float thrSPL;

        public References()
        {
            refVal = 0;
            maxVal = float.PositiveInfinity;
            dynamicRange = float.NaN;
            thrSPL = float.NaN;
        }
        public References(float refVal)
        {
            this.refVal = refVal;
            maxVal = float.PositiveInfinity;
            dynamicRange = float.NaN;
            thrSPL = float.NaN;
        }
        public References(float refVal, float maxVal)
        {
            this.refVal = refVal;
            this.maxVal = maxVal;
            dynamicRange = float.NaN;
            thrSPL = float.NaN;
        }

        public References Offset(float modRefCorr)
        {
            References r = new References(refVal + modRefCorr, maxVal + modRefCorr);
            r.dynamicRange = dynamicRange;
            r.thrSPL = thrSPL + modRefCorr;
            return r;
        }

        public override string ToString()
        {
            return "Ref: " + refVal.ToString("F1") + "; Max: " + maxVal.ToString("F1");
        }

    }
}
