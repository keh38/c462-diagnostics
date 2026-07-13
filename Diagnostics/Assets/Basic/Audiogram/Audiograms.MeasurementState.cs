using System.Collections.Generic;

using UnityEngine;

using C462.Shared;

namespace Audiograms
{
    public class StimulusCondition
    {
        public float Frequency { get; set; }

        public Laterality Laterality { get; set; }

        public bool Completed { get; set; }

        public float Threshold { get; set; }

        public bool NewEar {  get; set; }
        public bool NewFrequency { get; set; }

        public int NumTries { get; set; }

        public StimulusCondition() { }

        public StimulusCondition(float frequency, Laterality laterality, bool newEar, bool newFrequency)
        {
            Frequency = frequency;
            Laterality = laterality;
            Completed = false;
            Threshold = float.NaN;

            NewEar = newEar;
            NewFrequency = newFrequency;

            NumTries = 0;
        }
    }

    public class MeasurementState
    {
        public List<StimulusCondition> stimulusConditions;

        public MeasurementState() { }

        public MeasurementState(float[] freqs, AudiogramTestEar ears)
        {
            stimulusConditions = new List<StimulusCondition>();
            for (int k=0; k<freqs.Length; k++)
            {
                bool newFreq = k > 0;
                if (ears != AudiogramTestEar.Right)
                {
                    stimulusConditions.Add(
                        new StimulusCondition(
                        frequency: freqs[k], 
                        laterality: Laterality.Left,
                        newFrequency: newFreq,
                        newEar: false
                        ));
                    newFreq = false;
                }
                if (ears != AudiogramTestEar.Left)
                {
                    stimulusConditions.Add(
                        new StimulusCondition(
                        frequency: freqs[k],
                        laterality: Laterality.Right,
                        newFrequency: newFreq,
                        newEar: ears == AudiogramTestEar.Both
                        ));
                }
            }
        }

        public void SetCompleted(StimulusCondition stimCon, float threshold)
        {
            var sc = stimulusConditions.Find(x => x == stimCon);
            sc.Completed = true;
            sc.Threshold = threshold;
        }

        public StimulusCondition GetNext()
        {
            var sc = stimulusConditions.Find(x => !x.Completed);
            return sc;
        }

        public int NumStimulusConditions { get { return stimulusConditions.Count; } }

        public int NumCompleted { get { return stimulusConditions.FindAll(x => x.Completed).Count; } }

        public bool IsCompleted { get { return stimulusConditions.FindAll(x => !x.Completed).Count == 0; } }

        public int PercentComplete { get { return Mathf.RoundToInt(100f * NumCompleted / NumStimulusConditions); } }
    }
}