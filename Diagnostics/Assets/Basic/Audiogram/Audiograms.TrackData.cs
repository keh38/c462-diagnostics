using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using KLib;
using KLib.Signals;
using KLib.Signals.Enumerations;
using KLib.Signals.Waveforms;

using Newtonsoft.Json;
using ProtoBuf;

namespace Audiograms
{
    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class TrackData
    {
        public Laterality ear;
        public float frequency;
        public float maskerLevel;
        public float[] signalLevel;
        public float[] responseTime_s;
        public bool[] detected;
        public float thresholdSPL = float.NaN;
        public bool alternateComputation;
        public List<float> reversals;

        [JsonIgnore]
        private int index;
        [JsonIgnore]
        private int lengthIncrement;

        public TrackData() { }
        
        public TrackData(Laterality ear, float freq, int lengthIncrement)
        {
            this.ear = ear;
            this.frequency = freq;
            this.maskerLevel = float.NaN;

            this.lengthIncrement = lengthIncrement;
            signalLevel = new float[lengthIncrement];
            responseTime_s = new float[lengthIncrement];
            detected = new bool[lengthIncrement];
            thresholdSPL = float.NaN;
            alternateComputation = false;

            reversals = new List<float>();
            index = 0;
        }
        
        public int Length()
        {
            return index;
        }
        
        public void Add(float signallevel, float maskerLevel, float responseTime_s, bool detected)
        {
            if (index == this.signalLevel.Length)
            {
                int newLen = this.signalLevel.Length + lengthIncrement;
                System.Array.Resize(ref this.signalLevel, newLen); 
                System.Array.Resize(ref this.responseTime_s, newLen); 
                System.Array.Resize(ref this.detected, newLen); 
            }
            this.signalLevel[index] = signallevel;
            this.responseTime_s[index] = responseTime_s;
            this.detected[index] = detected;
            ++index;
        }
        
        public void Trim()
        {
            System.Array.Resize(ref this.signalLevel, index); 
            System.Array.Resize(ref this.responseTime_s, index); 
            System.Array.Resize(ref this.detected, index); 
        }

        public void AddReversal(float reversal)
        {
            this.reversals.Add(reversal);
        }
    }
}