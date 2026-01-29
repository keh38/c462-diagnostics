using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpeechReception
{
    [JsonObject(MemberSerialization.OptOut)]
    public class MatrixTestData
    {
        [JsonObject(MemberSerialization.OptOut)]
        public class Response
        {
            public string sentence;
            public string answer;
            public bool volumeChanged;
            public float stimLevel;
            public float maskerLevel;
            public float SNR;
            public int numCorrect;

            public Response()
            {
            }
            public Response(string sentence, string answer, int numCorrect, float stimLevel, float maskerLevel, bool volumeChanged)
            {
                this.sentence = sentence;
                this.answer = answer;
                this.numCorrect = numCorrect;
                this.stimLevel = stimLevel;
                this.maskerLevel = maskerLevel;
                this.SNR = stimLevel - maskerLevel;
                this.volumeChanged = volumeChanged;
            }
        }

        public string date;
        public string test;
        public int runNumber;
        public TestEar testEar;
        public float maskerLevel;
        public float stimLevel;
        public MatrixTestMode mode;

        private bool _passedCriteria;

        public List<Response> responses = new List<Response>();

        public MatrixTestData() { }
        public MatrixTestData(float stimLevel, float maskerLevel, MatrixTestMode mode)
        {
            this.stimLevel = stimLevel;
            this.maskerLevel = maskerLevel;
            this.mode = mode;
        }

        public int AddResponse(string sentence, string answer, float stimLevel, float maskerLevel, bool volumeChanged)
        {
            int numCorrect = MatrixTest.CountNumberCorrect(sentence, answer);
            responses.Add(new Response(sentence, answer, numCorrect, stimLevel, maskerLevel, volumeChanged));
            return numCorrect;
        }

    }
}