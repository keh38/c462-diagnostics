using System;
using System.Collections;
using System.Collections.Generic;

using ProtoBuf;
namespace SpeechReception
{
    [System.Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PerformanceCriteria
    {
        public float allowablePctRange = 10;
        public int minBlocks = 2;
        public int maxBlocks = 3;

        public PerformanceCriteria() { }

    }
}