using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace CombinedAudioLDL
{
    [JsonObject(MemberSerialization.OptOut)]
    public class CombinedSliderLog
    {
        public float[] t;
        public float[] position;
        public float[] value;
        public int[] button;

        [JsonIgnore]
        private int _index;
        [JsonIgnore]
        private int _lengthIncrement;
        [JsonIgnore]
        private bool _finished = false;

        public CombinedSliderLog() : this(10000) { }

        public CombinedSliderLog(int lengthIncrement)
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
            t = new float[_lengthIncrement];
            position = new float[_lengthIncrement];
            value = new float[_lengthIncrement];
            button = new int[_lengthIncrement];
            _index = 0;
        }

        public void Add(float position, float value)
        {
            if (_finished) return;

            if (_index == this.t.Length)
            {
                int newLen = this.t.Length + _lengthIncrement;
                System.Array.Resize(ref this.t, newLen);
                System.Array.Resize(ref this.position, newLen);
                System.Array.Resize(ref this.value, newLen);
                System.Array.Resize(ref this.button, newLen);
            }
            this.t[_index] = Time.timeSinceLevelLoad;
            this.position[_index] = position;
            this.value[_index] = value;
            this.button[_index] = -1;

            ++_index;
        }

        public void Add(int button)
        {
            if (_finished) return;
            if (_index == this.t.Length)
            {
                int newLen = this.t.Length + _lengthIncrement;
                System.Array.Resize(ref this.t, newLen);
                System.Array.Resize(ref this.position, newLen);
                System.Array.Resize(ref this.value, newLen);
                System.Array.Resize(ref this.button, newLen);
            }
            this.t[_index] = Time.timeSinceLevelLoad;
            this.position[_index] = float.NaN;
            this.value[_index] = float.NaN;
            this.button[_index] = button;

            _index++;
        }

        public CombinedSliderLog Finish()
        {
            System.Array.Resize(ref this.t, _index);
            System.Array.Resize(ref this.position, _index);
            System.Array.Resize(ref this.value, _index);
            System.Array.Resize(ref this.button, _index);

            _finished = true;

            return this;
        }

    }

}