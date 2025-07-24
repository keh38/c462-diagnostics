using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Bekesy
{ 
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class TrackLog
    {
        public double[] t;
        public float[] level;
        public int[] direction;
        public int[] reversal;

        [JsonIgnore]
        private int _index;
        [JsonIgnore]
        private int _lengthIncrement;

        public TrackLog() : this(10000) { }

        public TrackLog(int lengthIncrement)
        {
            _lengthIncrement = lengthIncrement;
            Clear();
        }

        [ProtoIgnore]
        [JsonIgnore]
        public int Length { get { return _index; } }

        public void Clear()
        {
            t = new double[_lengthIncrement];
            level = new float[_lengthIncrement];
            direction = new int[_lengthIncrement]; 
            reversal = new int[_lengthIncrement];
            _index = 0;
        }

        public void Add(double t, float level, int direction, int reversal)
        {
            if (_index == this.t.Length)
            {
                int newLen = this.t.Length + _lengthIncrement;
                System.Array.Resize(ref this.t, newLen);
                System.Array.Resize(ref this.level, newLen);
                System.Array.Resize(ref this.direction, newLen);
                System.Array.Resize(ref this.reversal, newLen);
            }
            this.t[_index] = t;
            this.level[_index] = level;
            this.direction[_index] = direction;
            this.reversal[_index] = reversal;

            _index++;
        }

        public void Trim()
        {
            System.Array.Resize(ref this.t, _index);
            System.Array.Resize(ref this.level, _index);
            System.Array.Resize(ref this.direction, _index);
            System.Array.Resize(ref this.reversal, _index);
        }
    }

}