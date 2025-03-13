using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turandot.Schedules
{
    public class TrackState
    {
        public float value;
        public float lastValue;
        public int numReverse;
        public int numCorrect;
        public int lastDirection;
        public string type;
        public float finalValue;
        public bool isFinished;
        public List<AdaptHistory> history = new List<AdaptHistory>();
        public List<float[]> catchValues = new List<float[]>();
        public int ncatchesDone;

        public int ntracksDone;

        public TrackState() { }

        public void Initialize(float startVal, int ncatchToStart)
        {
            value = lastValue = startVal;
            numCorrect = numReverse = lastDirection = 0;

            ncatchesDone = 0;
            type = ncatchToStart > 0 ? "catch" : "test";
            finalValue = float.NaN;
            isFinished = false;
            catchValues.Clear();
            history.Clear();
        }
    }
}
