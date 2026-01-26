using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Digits
{
    [JsonObject(MemberSerialization.OptOut)]
    public class TestData
    {
        public string type;
        public string name;
        public float SNR;
        public float ITD;
        public int numDigitsTested;
        public int numDigitsCorrect;
        public List<Block> blocks = new List<Block>();

        private TestSpec.TestType _testType;

        public TestData() { }

        public TestData(TestSpec testSpec, int testNum)
        {
            _testType = testSpec.type;

            type = testSpec.type.ToString();
            name = $"{testNum:D2}-{type}";
            SNR = testSpec.SNR;
            ITD = testSpec.ITD;
            numDigitsTested = 0;
            numDigitsCorrect = 0;
        }

        public float FractionCorrect()
        {
            return (numDigitsTested > 0) ? (float)numDigitsCorrect / (float)numDigitsTested : -1;
        }

        public void NewBlock()
        {
            blocks.Add(new Block(this.SNR, this.ITD, _testType));
        }

        public void AddTrial(Trial trialData)
        {
            blocks[blocks.Count - 1].AddTrial(trialData);
            numDigitsTested += trialData.Response.Length;
            numDigitsCorrect += trialData.NumCorrect();
        }

    }
}