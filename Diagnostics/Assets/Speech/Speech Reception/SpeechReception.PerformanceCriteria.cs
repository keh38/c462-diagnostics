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
        private bool ShouldSerializeAllowablePctRange() { return false; }

        [PropertyOrder(2)]
        public int MinBlocks { get; set; }
        private bool ShouldSerializeMinBlocks() { return false; }

        [PropertyOrder(3)]
        public int MaxBlocks { get; set; }
        private bool ShouldSerializeMaxBlocks() { return false; }

        public PerformanceCriteria()
        {
            Apply = false;
            AllowablePctRange = 10;
            MinBlocks = 2;
            MaxBlocks = 3;
        }

    }
}