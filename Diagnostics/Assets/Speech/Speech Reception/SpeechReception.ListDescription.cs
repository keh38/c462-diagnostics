using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using KLib;
using KLib.Signals.Enumerations;
using ExtensionMethods;

namespace SpeechReception
{
    [Serializable]
    public class ListDescription
    {
        [Serializable]
        public class Sentence
        {
            public string wavfile;
            public float SNR = float.NaN;
            public string whole;
            public List<string> words = new List<string>();

            public Sentence() { }

            public Sentence(Sentence source, float SNR)
            {
                this.wavfile = source.wavfile;
                this.SNR = SNR;
                this.whole = source.whole;
                foreach (string w in source.words) this.words.Add(w);
            }
        }

        public string title;
        public string type;

        public List<Sentence> sentences = new List<Sentence>();

        public void SetSequence(Sequence sequence, float[] SNRs)
        {
            List<Sentence> seq = new List<Sentence>();

            if (sequence.choose > 0)
            {
                List<Sentence> keep = new List<Sentence>();
                foreach (int isen in KMath.Permute(sentences.Count, sequence.choose)) keep.Add(sentences[isen]);
                sentences = keep;
            }

            foreach (int isnr in KMath.Permute(SNRs.Length))
            {
                if (sequence.Order == Order.Sequential)
                {
                    for (int kb = 0; kb < sequence.NumBlocks; kb++)
                    { 
                        for (int k = 0; k < sequence.RepeatsPerBlock; k++)
                        {
                            foreach (Sentence s in sentences)
                            {
                                seq.Add(new Sentence(s, SNRs[isnr]));
                            }
                        }
                    }
                }

                else if (sequence.Order == Order.FullRandom)
                {
                    foreach (int isen in KMath.Permute(sentences.Count, sequence.NumBlocks * sequence.RepeatsPerBlock * sentences.Count))
                    {
                         seq.Add(new Sentence(sentences[isen], SNRs[isnr]));
                    }
                }

                else if (sequence.Order == Order.BlockRandom)
                {
                    for (int kb = 0; kb < sequence.NumBlocks; kb++)
                    {
                        for (int k = 0; k < sequence.RepeatsPerBlock; k++)
                        {
                            foreach (int ioff in KMath.Permute(sentences.Count))
                            {
                                seq.Add(new Sentence(sentences[ioff], SNRs[isnr]));
                            }
                        }
                    }
                }

            }

            sequence.ItemsPerBlock = sequence.RepeatsPerBlock * sentences.Count;
            sentences = seq;
        }

    }
}