using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Inputs
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class GrapherLog
    {
        public string name;
        public float[] t;
        public float[] taudio;
        public float[] y;
        public int[] u;

        [JsonIgnore]
        private int _index;
        [JsonIgnore]
        private int _lengthIncrement;

        public GrapherLog() : this("grapher", 10000)
        {
        }

        public GrapherLog(string name) : this(name, 10000)
        {
        }

        public GrapherLog(string name, int lengthIncrement)
        {
            this.name = name;
            _lengthIncrement = lengthIncrement;
        }

        [JsonIgnore]
        public int Length
        {
            get { return _index; }
        }

        public void Initialize(string name)
        {
            this.name = name;
            Clear();
        }

        public void Clear()
        {
            t = new float[_lengthIncrement];
            taudio = new float[_lengthIncrement];
            y = new float[_lengthIncrement];
            u = new int[_lengthIncrement];
            _index = 0;
        }

        public void Add(float t, float taudio, float value, int user)
        {
            if (_index == this.t.Length)
            {
                int newLen = this.t.Length + _lengthIncrement;
                System.Array.Resize(ref this.t, newLen);
                System.Array.Resize(ref this.taudio, newLen);
                System.Array.Resize(ref this.y, newLen);
                System.Array.Resize(ref this.u, newLen);
            }
            this.t[_index] = t;
            this.taudio[_index] = taudio;
            this.y[_index] = value;
            this.u[_index] = user;

            ++_index;
        }

        public void Trim()
        {
            System.Array.Resize(ref this.t, _index);
            System.Array.Resize(ref this.taudio, _index);
            System.Array.Resize(ref this.y, _index);
            System.Array.Resize(ref this.u, _index);
        }
    }

}