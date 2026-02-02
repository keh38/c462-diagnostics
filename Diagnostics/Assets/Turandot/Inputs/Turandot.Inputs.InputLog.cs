using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Turandot.Inputs
{
    [JsonObject(MemberSerialization.OptOut)]
    public class InputLog
    {
        public string name;
        public float[] t;
        public float[] value;

        [JsonIgnore]
        private int _index;
        [JsonIgnore]
        private int _lengthIncrement;

        public InputLog() : this("cue", 10000)
        {
        }

        public InputLog(string name) : this(name, 10000)
        {
        }

        public InputLog(string name, int lengthIncrement)
        {
            this.name = name;
            _lengthIncrement = lengthIncrement;
            Clear();
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
            value = new float[_lengthIncrement];
            _index = 0;
        }

        public void Add(float t, float value)
        {
            if (_index == this.t.Length)
            {
                int newLen = this.t.Length + _lengthIncrement;
                System.Array.Resize(ref this.t, newLen);
                System.Array.Resize(ref this.value, newLen);
            }
            this.t[_index] = t;
            this.value[_index] = value;

            ++_index;
        }

        public void Trim()
        {
            System.Array.Resize(ref this.t, _index);
            System.Array.Resize(ref this.value, _index);
        }

        [JsonIgnore]
        public string JSONString
        {
            get
            {
                Trim();
                return KLib.FileIO.JSONSerializeToString(this);
            }
        }

    }

}