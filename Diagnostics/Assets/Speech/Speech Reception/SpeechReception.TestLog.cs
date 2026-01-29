using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace SpeechReception
{
    [JsonObject(MemberSerialization.OptOut)]
    public class TestLog
    {
        public float[] t;
        public float[] tunity;
        public string[] message;

        [JsonIgnore]
        private int _index;
        [JsonIgnore]
        private int _lengthIncrement;

        public TestLog() : this(1000) { }

        public TestLog(int lengthIncrement)
        {
            this._lengthIncrement = lengthIncrement;
            Clear();
        }

        public void Clear()
        {
            t = new float[_lengthIncrement];
            tunity = new float[_lengthIncrement];
            message = new string[_lengthIncrement];
            _index = 0;
        }

        public int Length()
        {
            return _index;
        }

        public void Add(string message)
        {
            Add(Time.timeSinceLevelLoad, Time.realtimeSinceStartup, message);
        }

        public void Add(float t, float tunity, string message)
        {
            if (_index == this.t.Length)
            {
                int newLen = this.t.Length + _lengthIncrement;
                System.Array.Resize(ref this.t, newLen);
                System.Array.Resize(ref this.tunity, newLen);
                System.Array.Resize(ref this.message, newLen);
            }
            this.t[_index] = t;
            this.tunity[_index] = tunity;
            this.message[_index] = message;
            ++_index;
        }

        public TestLog Trim()
        {
            System.Array.Resize(ref this.t, _index);
            System.Array.Resize(ref this.tunity, _index);
            System.Array.Resize(ref this.message, _index);
            return this;
        }

    }

}