using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Digits
{
    [JsonObject(MemberSerialization.OptOut)]
    public class Block
    {
        public float SNR;
        public float ITD;
        public int numDigitsTested;
        public int numDigitsCorrect;
        public TestSpec.TestType type;
        public List<Trial> trials = new List<Trial>();

        public Block()
        {
        }
        public Block(float SNR, float ITD, TestSpec.TestType type)
        {
            this.SNR = SNR;
            this.ITD = ITD;
            this.type = type;
        }

        public void AddTrial(Trial trialData)
        {
            trials.Add(trialData);
            numDigitsTested += trialData.Response.Length;
            numDigitsCorrect += trialData.NumCorrect();
        }
    }
}