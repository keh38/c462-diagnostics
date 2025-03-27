using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class AudioLog
    {
        public double[] t;
        public string[] message;

        [JsonIgnore]
        private int _index;
        [JsonIgnore]
        private int _lengthIncrement;

        public AudioLog() : this(1000)
        {
        }

        public AudioLog(int lengthIncrement)
        {
            this._lengthIncrement = lengthIncrement;
            Clear();
        }

        public void Clear()
        {
            t = new double[_lengthIncrement];
            message = new string[_lengthIncrement];
            _index = 0;
        }

        public int Length()
        {
            return _index;
        }

        public void Add(double t, string message)
        {
            if (_index == this.t.Length)
            {
                int newLen = this.t.Length + _lengthIncrement;
                System.Array.Resize(ref this.t, newLen);
                System.Array.Resize(ref this.message, newLen);
            }
            this.t[_index] = t;
            this.message[_index] = message;
            ++_index;
            Debug.Log(message);
        }

        public AudioLog Trim()
        {
            System.Array.Resize(ref this.t, _index);
            System.Array.Resize(ref this.message, _index);
            return this;
        }

    }

}