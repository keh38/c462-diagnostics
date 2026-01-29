using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using KLib.TypeConverters;
using OrderedPropertyGrid;

namespace SpeechReception
{
    [TypeConverter(typeof(PerformanceCriteriaConverter))]
    public class PerformanceCriteria
    {
        [Browsable(false)]
        public bool Apply { get; set; }
        private bool ShouldSerializeApply() { return false; }   

        [PropertyOrder(1)]  
        public float AllowablePctRange { get; set; }
        public bool ShouldSerializeAllowablePctRange() { return Apply; }

        [PropertyOrder(2)]
        public int MinBlocks { get; set; }
        public bool ShouldSerializeMinBlocks() { return Apply; }

        [PropertyOrder(3)]
        public int MaxBlocks { get; set; }
        public bool ShouldSerializeMaxBlocks() { return Apply; }

        public PerformanceCriteria()
        {
            Apply = false;
            AllowablePctRange = 10;
            MinBlocks = 2;
            MaxBlocks = 3;
        }

    }
}