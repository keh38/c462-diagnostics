using System;
using System.Collections;
using System.Collections.Generic;

using ProtoBuf;
namespace SpeechReception
{
    [System.Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class ClosedSet
    {
        public enum FeedbackType { None, Investigator, Subject}

        public bool active = false;
        public bool shuffle = false;
        public int numRows = -1;
        public FeedbackType feedback = FeedbackType.None;
        public List<string> decoys = new List<string>();
        public PerformanceCriteria performanceCriteria = null;
    }
}