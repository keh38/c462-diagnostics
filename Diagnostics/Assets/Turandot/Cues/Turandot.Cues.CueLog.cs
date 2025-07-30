using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Cues
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class CueLog
    {
        public float[] t;
        public int[] value;

        [JsonIgnore]
        private int _index;
        [JsonIgnore]
        private int _lengthIncrement;

        public CueLog() : this(10000) { }

        public CueLog(int lengthIncrement)
        {
            _lengthIncrement = lengthIncrement;
            Clear();
        }

        public void Clear()
        {
            t = new float[_lengthIncrement];
            value = new int[_lengthIncrement];
            _index = 0;
        }

        public void Add(float t, bool value)
        {
            if (_index == this.t.Length)
            {
                int newLen = this.t.Length + _lengthIncrement;
                System.Array.Resize(ref this.t, newLen);
                System.Array.Resize(ref this.value, newLen);
            }
            this.t[_index] = t;
            this.value[_index] = value ? 1 : 0;

            ++_index;
        }

        public void Trim()
        {
            System.Array.Resize(ref this.t, _index);
            System.Array.Resize(ref this.value, _index);
        }

    }

}