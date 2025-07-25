﻿using KLib;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LDL
{
    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
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

        [ProtoIgnore]
        public int NumCompleted { get { return NumConditions - testOrder.Count; } }

        [ProtoIgnore]
        public float FractionCompleted { get { return (float)NumCompleted / NumConditions; } }

        [ProtoIgnore]
        public int PercentCompleted { get { return Mathf.RoundToInt(100 * FractionCompleted); } }

        [ProtoIgnore]
        public bool IsComplete { get { return testOrder.Count == 0;} }
    }
}
