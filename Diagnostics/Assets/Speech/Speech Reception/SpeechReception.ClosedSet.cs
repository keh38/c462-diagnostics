using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using KLib.TypeConverters;
using OrderedPropertyGrid;

namespace SpeechReception
{
    [TypeConverter(typeof(ClosedSetConverter))]
    public class ClosedSet
    {
        public enum FeedbackType { None, Investigator, Subject}

        [PropertyOrder(0)]
        public FeedbackType Feedback { get; set; }
        private bool ShouldSerializeFeedback() { return false; }

        [PropertyOrder(1)]
        public List<string> Decoys { get; set; } 
        public bool ShouldSerializeDecoys() { return Decoys!=null && Decoys.Count > 0; }

        [PropertyOrder(2)]
        [DisplayName("Performance")]
        public PerformanceCriteria PerformanceCriteria { get; set; } 
        private bool ShouldSerializePerformanceCriteria() { return false; }

        public ClosedSet()
        {
            Feedback = FeedbackType.None;
            Decoys = new List<string>();
            PerformanceCriteria = new PerformanceCriteria();
        }

        [XmlIgnore]
        public bool active = false;
        [XmlIgnore]
        public bool shuffle = false;
        [XmlIgnore]
        public int numRows = -1;
    }
}