using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ProtoBuf;

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

        [ProtoIgnore]
        public int NumConditions { get { return testConditions.Count; } }

        [ProtoIgnore]
        public int NumCompleted { get { return NumConditions - testOrder.Count; } }
    }
}
