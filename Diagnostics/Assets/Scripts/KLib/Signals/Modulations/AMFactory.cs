using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KLib.Signals.Enumerations;

namespace KLib.Signals.Modulations
{
    public static class AMFactory
    {
        public static List<AM> OneOfEach()
        {
            List<AM> am = new List<AM>();
            foreach (AMShape s in Enum.GetValues(typeof(AMShape)))
            {
                am.Add(Create(s));
            }
            return am;
        }

        public static AM Create(AMShape shape)
        {
            AM am = null;
            switch (shape)
            {
                case AMShape.None:
                    am = new AM();
                    break;
                case AMShape.Sinusoidal:
                    am = new SinusoidalAM();
                    break;
            }
            return am;
        }

        public static AM Create(string shape)
        {
            AM am = new AM();
            switch (shape)
            {
                case "Sinusoidal":
                case "SAM":
                    am = new SinusoidalAM();
                    break;
            }
            return am;
        }

    }
}
