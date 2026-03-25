using System;
using System.Collections;
using System.Collections.Generic;

namespace SpeechReception
{
    public class PerformanceCriteria
    {
        public bool Apply { get; set; }
        private bool ShouldSerializeApply() { return false; }   

        public float AllowablePctRange { get; set; }
        public bool ShouldSerializeAllowablePctRange() { return Apply; }

        public int MinBlocks { get; set; }
        public bool ShouldSerializeMinBlocks() { return Apply; }

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