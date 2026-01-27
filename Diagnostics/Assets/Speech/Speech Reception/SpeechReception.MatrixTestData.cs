using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using ProtoBuf;

using KLib.Signals.Enumerations;

namespace SpeechReception
{
/*    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class MatrixTestData
    {
        [Serializable]
        [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
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
        public Laterality laterality;
        public float maskerLevel;
        public float stimLevel;
        public MatrixTest.Mode mode;

        private bool _passedCriteria;

        public List<Response> responses = new List<Response>();

        public MatrixTestData() { }
        public MatrixTestData(float stimLevel, float maskerLevel, MatrixTest.Mode mode)
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

    }*/
}