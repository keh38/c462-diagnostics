using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SpeechReception
{
    public class ClosedSet
    {
        public enum FeedbackType { None, Investigator, Subject}

        public FeedbackType Feedback { get; set; }
        private bool ShouldSerializeFeedback() { return false; }

        public List<string> Decoys { get; set; } 
        public bool ShouldSerializeDecoys() { return Decoys!=null && Decoys.Count > 0; }

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