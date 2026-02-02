using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Audiograms
{
    [JsonObject(MemberSerialization.OptOut)]
    public class AudiogramButtonLog
    {
        public float[] tbuttonpress;

        [JsonIgnore]
        private int _index;
        [JsonIgnore]
        private int _lengthIncrement;

        public AudiogramButtonLog() : this(10000) { }

        public AudiogramButtonLog(int lengthIncrement)
        {
            _lengthIncrement = lengthIncrement;
            Clear();
        }

        [JsonIgnore]
        public int Length
        {
            get { return _index; }
        }

        public void Clear()
        {
            tbuttonpress = new float[_lengthIncrement];
            _index = 0;
        }

        public void Add()
        {
            if (_index == this.tbuttonpress.Length)
            {
                int newLen = this.tbuttonpress.Length + _lengthIncrement;
                System.Array.Resize(ref this.tbuttonpress, newLen);
            }
            this.tbuttonpress[_index] = Time.realtimeSinceStartup;

            ++_index;
        }

        public AudiogramButtonLog Trim()
        {
            System.Array.Resize(ref this.tbuttonpress, _index);

            return this;
        }

    }

}