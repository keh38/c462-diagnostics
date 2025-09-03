using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class History
    {
        public float[] t;
        public float[] tunity;
        public string[] type;
        public string[] message;

        [JsonIgnore]
        private int _index;
        [JsonIgnore]
        private int _lengthIncrement;

        public History() : this(1000)
        {
        }

        public History(int lengthIncrement)
        {
            this._lengthIncrement = lengthIncrement;
            Clear();
        }

        public void Clear()
        {
            t = new float[_lengthIncrement];
            type = new string[_lengthIncrement];
            message = new string[_lengthIncrement];
            _index = 0;
        }

        public int Length()
        {
            return _index;
        }

        public void Add(float t, float tunity, HistoryEvent type)
        {
            Add(t, tunity, type, "");
        }

        public void Add(float t, float tunity, HistoryEvent type, string message)
        {
            if (_index == this.t.Length)
            {
                int newLen = this.t.Length + _lengthIncrement;
                System.Array.Resize(ref this.t, newLen);
                System.Array.Resize(ref this.tunity, newLen);
                System.Array.Resize(ref this.type, newLen);
                System.Array.Resize(ref this.message, newLen);
            }
            this.t[_index] = t;
            this.tunity[_index] = tunity;
            this.type[_index] = type.ToString();
            this.message[_index] = message;
            ++_index;
        }

        public void Trim()
        {
            System.Array.Resize(ref this.t, _index);
            System.Array.Resize(ref this.tunity, _index);
            System.Array.Resize(ref this.type, _index);
            System.Array.Resize(ref this.message, _index);
        }

    }

}