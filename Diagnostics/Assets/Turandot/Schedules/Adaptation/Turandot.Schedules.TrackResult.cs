using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turandot.Schedules
{
    public class TrackResult
    {
        public string name;
        public float threshold;

        public TrackResult() { }

        public TrackResult(string name, float threshold)
        {
            this.name = name;
            this.threshold = threshold;
        }
    }
}
