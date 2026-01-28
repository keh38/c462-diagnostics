using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SpeechReception
{
    public class MatrixTest
    {
        public enum Mode { VarySignal, VaryMasker}

        public bool active = false;
        public Mode mode = Mode.VarySignal;
        public float maskerLevel = 65;
        public float stimLevel = 65;
        public float startSNR = 0;

        private int _numReversals;
        private int _lastDir;

        public float StimLevel
        {
            get { return stimLevel; }
        }

        public float SNR
        {
            get { return stimLevel - maskerLevel; }
        }

        public void Initialize()
        {
            Debug.Log("Matrix test mode: " + mode);
            if (mode == Mode.VarySignal)
            {
                stimLevel = maskerLevel + startSNR;
            }
            else if (mode == Mode.VaryMasker)
            {
                maskerLevel = stimLevel - startSNR;
            }

            _numReversals = 0;
            _lastDir = 0;
        }

        public void UpdateSNR(int numCorrect)
        {
            float f = 1.5f * Mathf.Pow(1.41f, -(float)_numReversals);
            f = Mathf.Max(f, 0.1f);
            float slope = 0.15f;
            float ptarget = 0.5f;
            float pc = (float)numCorrect / 5.0f;
            float delta = -(f * (pc - ptarget)) / slope;

            bool isReversal = (delta < 0 && _lastDir > 0) || (delta > 0 && _lastDir < 0);
            _lastDir = (int) Mathf.Sign(delta);

            if (mode == Mode.VarySignal)
            {
                stimLevel += delta;
            }
            else if (mode == Mode.VaryMasker)
            {
                maskerLevel -= delta;
            }

            if (isReversal)
            {
                _numReversals++;
                Debug.Log("Reversal!!!");
            }
            Debug.Log("delta = " + delta + "; numReversals = " + _numReversals);
        }

        public static string WordListToString(List<string> words)
        {
            string sentence = words[0];
            for (int k=1; k<words.Count; k++)
            {
                sentence += " " + words[k].ToLower();
            }
            return sentence;
        }

        public static List<string> StringToWordList(string sentence)
        {
            var words = sentence.Split(' ');
            return new List<string>(words);
        }

        public static int CountNumberCorrect(string sentence, string answer)
        {
            var correctWords = StringToWordList(sentence);
            var answerWords = StringToWordList(answer);

            int numCorrect = 0;
            for (int k=0; k<correctWords.Count; k++)
            {
                if (answerWords[k].ToLower().Equals(correctWords[k].ToLower())) numCorrect++;
            }
            return numCorrect;
        }
    }
}