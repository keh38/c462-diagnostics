using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using UnityEngine;

using OrderedPropertyGrid;
using System.Xml.Serialization;

namespace SpeechReception
{
    public enum MatrixTestMode { VarySignal, VaryMasker }

    [TypeConverter(typeof(MatrixTestTypeConverter))]
    public class MatrixTest
    {
        [XmlIgnore]
        [Browsable(false)]
        public bool Active { get; private set; } = false;

        [Browsable(false)]
        public MatrixTestMode Mode { get; set; } = MatrixTestMode.VarySignal;

        [PropertyOrder(1)]
        public float MaskerLevel { get; set; } = 65;
        private bool ShouldSerializeMaskerLevel() { return false; }

        [PropertyOrder(2)]
        public float StimLevel { get; set; } = 65;
        private bool ShouldSerializeStimLevel() { return false; }

        [PropertyOrder(3)]
        public float StartSNR { get; set; } = 0;
        private bool ShouldSerializeStartSNR() { return false; }


        private int _numReversals;
        private int _lastDir;

        [XmlIgnore]
        [Browsable(false)]
        public float SNR
        {
            get { return StimLevel - MaskerLevel; }
        }

        public void Initialize()
        {
            Debug.Log("Matrix test mode: " + Mode);
            if (Mode == MatrixTestMode.VarySignal)
            {
                StimLevel = MaskerLevel + StartSNR;
            }
            else if (Mode == MatrixTestMode.VaryMasker)
            {
                MaskerLevel = StimLevel - StartSNR;
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

            if (Mode == MatrixTestMode.VarySignal)
            {
                StimLevel += delta;
            }
            else if (Mode == MatrixTestMode.VaryMasker)
            {
                MaskerLevel -= delta;
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