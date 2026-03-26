using KLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LDL
{
    public class MeasurementState
    {
        public List<int> testOrder;
        public List<TestCondition> testConditions;

        public MeasurementState()
        {
            testOrder = new List<int>();
            testConditions = new List<TestCondition>();
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

        public int NumConditions { get; set; }

        public int NumCompleted { get { return NumConditions - testOrder.Count; } }

        public float FractionCompleted { get { return (float)NumCompleted / NumConditions; } }

        public int PercentCompleted { get { return Mathf.RoundToInt(100 * FractionCompleted); } }

        public bool IsComplete { get { return testOrder.Count == 0;} }
    }
}
