using System.Collections.Generic;

using ProtoBuf;
using KLib.Signals.Enumerations;
using KLib.Signals;
using Microsoft.Graph;
using UnityEngine;

namespace Audiograms
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
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

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class MeasurementState
    {
        public List<StimulusCondition> stimulusConditions;

        public MeasurementState() { }

        public MeasurementState(float[] freqs, TestEar ears)
        {
            stimulusConditions = new List<StimulusCondition>();
            for (int k=0; k<freqs.Length; k++)
            {
                bool newFreq = k > 0;
                if (ears != TestEar.Right)
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
                if (ears != TestEar.Left)
                {
                    stimulusConditions.Add(
                        new StimulusCondition(
                        frequency: freqs[k],
                        laterality: Laterality.Right,
                        newFrequency: newFreq,
                        newEar: ears == TestEar.Both
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

        [ProtoIgnore]
        public int NumStimulusConditions { get { return stimulusConditions.Count; } }

        [ProtoIgnore]
        public int NumCompleted { get { return stimulusConditions.FindAll(x => x.Completed).Count; } }

        [ProtoIgnore]
        public bool IsCompleted { get { return stimulusConditions.FindAll(x => !x.Completed).Count == 0; } }

        [ProtoIgnore]
        public int PercentComplete { get { return Mathf.RoundToInt(100f * NumCompleted / NumStimulusConditions); } }
    }
}