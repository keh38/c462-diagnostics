using KLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LDL.Haptics
{
    [JsonObject(MemberSerialization.OptOut)]
    public class HapticsMeasurementState
    {
        public List<int> testOrder;
        public List<HapticsTestCondition> testConditions;

        public HapticsMeasurementState()
        {
            testOrder = new List<int>();
            testConditions = new List<HapticsTestCondition>();
        }

        public void CreateRandomTestOrder(int numRepeats)
        {
            NumConditions = numRepeats * testConditions.Count;

            testOrder.Clear();
            for (int k = 0; k < numRepeats; k++)
            {
                testOrder.AddRange(KMath.Permute(testConditions.Count));
            }
        }

        [JsonIgnore]
        public int NumConditions { get; set; }

        [JsonIgnore]
        public int NumCompleted { get { return NumConditions - testOrder.Count; } }

        [JsonIgnore]
        public float FractionCompleted { get { return (float)NumCompleted / NumConditions; } }

        [JsonIgnore]
        public int PercentCompleted { get { return Mathf.RoundToInt(100 * FractionCompleted); } }

        [JsonIgnore]
        public bool IsComplete { get { return testOrder.Count == 0;} }
    }
}
