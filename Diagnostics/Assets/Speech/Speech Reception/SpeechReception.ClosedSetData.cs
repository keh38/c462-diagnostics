using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using ProtoBuf;

using KLib.Signals.Enumerations;

namespace SpeechReception
{
/*    [Serializable]
    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    public class ClosedSetData
    {
        [Serializable]
        [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
        public class Response
        {
            public int block;
            public string word;
            public string answer;
            public bool volumeChanged;
            public float SNR;
            public bool correct;
            public Response()
            {
            }
            public Response(int blockNum, string word, string answer, float snr, bool volumeChanged)
            {
                this.block = blockNum;
                this.word = word;
                this.answer = answer;
                this.SNR = snr;
                this.volumeChanged = volumeChanged;
                this.correct = word == answer;
            }
        }
        
        public string date;
        public string test;
        public int runNumber;
        public bool isPractice;
        public Laterality laterality;
        public float percentCorrect = 0;
        public float[] pctCorrectByBlock = null;

        private int _curBlock;
        private int _itemsPerBlock;
        private int _itemsThisBlock;

        private PerformanceCriteria _performanceCriteria;
        private bool _passedCriteria;

        public List<Response> responses = new List<Response>();
        
        public ClosedSetData() { }
        public ClosedSetData(bool isPractice, int numBlocks, int itemsPerBlock, PerformanceCriteria performanceCriteria)
        {
            this.isPractice = isPractice;
            _curBlock = 0;
            _itemsPerBlock = itemsPerBlock;
            _itemsThisBlock = 0;
            pctCorrectByBlock = new float[numBlocks];
            for (int k = 0; k < numBlocks; k++) pctCorrectByBlock[k] = float.NaN;

            _performanceCriteria = performanceCriteria;
            _passedCriteria = false;
        }

        public bool PassedPerformanceCriteria
        {
            get { return _passedCriteria; }
        }

        public void AddResponse(string word, string answer, float snr, bool volumeChanged)
        {
            responses.Add(new Response(_curBlock, word, answer, snr, volumeChanged));

            if (++_itemsThisBlock == _itemsPerBlock)
            {
                var blockResponses = responses.FindAll(o => o.block == _curBlock);

                pctCorrectByBlock[_curBlock] = 100 * (float)blockResponses.FindAll(o => o.correct).Count / blockResponses.Count;
                _curBlock++;
                _itemsThisBlock = 0;

                if (_performanceCriteria != null && _curBlock >= _performanceCriteria.minBlocks)
                {
                    float pmin = float.PositiveInfinity;
                    float pmax = float.NegativeInfinity;
                    for (int k=0; k < _curBlock; k++)
                    {
                        pmin = Mathf.Min(pmin, pctCorrectByBlock[k]);
                        pmax = Mathf.Max(pmax, pctCorrectByBlock[k]);

                        _passedCriteria = (pmax - pmin) <= _performanceCriteria.allowablePctRange;
                    }
                }
            }

        }

        public void Finish()
        {
            percentCorrect = 100 * (float) responses.FindAll(o => o.correct).Count / responses.Count;
        }
    }
*/
}