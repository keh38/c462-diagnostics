using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Digits
{
    [JsonObject(MemberSerialization.OptOut)]
    public class TestSpec
    {
        public enum TestType { Practice, Test };
        public TestType type;
        public float SNR;
        public float ITD;
        public int numBlocks;
        public int numTrialsPerBlock;
        public float criterion;
        public int curBlock;

        public TestSpec(TestType type, float SNR, float ITD, int numBlocks, int numTrialsPerBlock, float criterion, int curBlock)
        {
            this.type = type;
            this.SNR = SNR;
            this.ITD = ITD;
            this.numBlocks = numBlocks;
            this.numTrialsPerBlock = numTrialsPerBlock;
            this.criterion = criterion;
            this.curBlock = curBlock;
        }
    }
}
