using System;
using System.Collections;
using System.Collections.Generic;

namespace SpeechReception
{
    [System.Serializable]
    public class Response
    {
        public string test;
        public int sentenceNum;
        public string sentence;
        public int Fs;
        public int attemptNum;
        public int numChannels;
        public string date;
        public float[] audio;
    }
}