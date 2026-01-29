using System;
using System.Collections;
using System.Collections.Generic;

using KLib.Signals.Enumerations;
using Newtonsoft.Json;

namespace SpeechReception
{
    [JsonObject(MemberSerialization.OptOut)]
    public class Data
    {
        [JsonObject(MemberSerialization.OptOut)]
        public class Response
        {
            public string sentence;
            public List<string> words;
            public string file;
            public bool volumeChanged;
            public float SNR;
            public Response()
            {
            }
            public Response(string sentence, List<string> words, float snr, bool volumeChanged, string file)
            {
                this.sentence = sentence;
                this.words = new List<string>(words);
                this.volumeChanged = volumeChanged;
                this.file = file;
                this.SNR = snr;
            }
        }
        
        public string date;
        public string test;
        public int Fs;
        public int runNumber;
        public bool isPractice;
        public TestEar testEar;

        public List<Response> responses = new List<Response>();

        public Data() { }
        public Data(bool isPractice)
        {
            this.isPractice = isPractice;
        }
    }
}