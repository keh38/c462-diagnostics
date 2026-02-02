using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace LDL
{
    [JsonObject(MemberSerialization.OptOut)]
    public class SliderLog
    {
        public float[] t;
        public float[] tunity;
        public float[] tdsp;
        public float[] position;
        public float[] value;

        [JsonIgnore]
        private int _index;
        [JsonIgnore]
        private int _lengthIncrement;

        public SliderLog() : this(10000) { }

        public SliderLog(int lengthIncrement)
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
            tunity = new float[_lengthIncrement];
            tdsp = new float[_lengthIncrement];
            position = new float[_lengthIncrement];
            value = new float[_lengthIncrement];
            _index = 0;
        }

        public void Add(float position, float value)
        {
            if (_index == this.t.Length)
            {
                int newLen = this.t.Length + _lengthIncrement;
                System.Array.Resize(ref this.t, newLen);
                System.Array.Resize(ref this.tunity, newLen);
                System.Array.Resize(ref this.tdsp, newLen);
                System.Array.Resize(ref this.position, newLen);
                System.Array.Resize(ref this.value, newLen);
            }
            this.t[_index] = Time.timeSinceLevelLoad;
            this.tunity[_index] = Time.realtimeSinceStartup;
            this.tdsp[_index] = (float) AudioSettings.dspTime;
            this.position[_index] = position;
            this.value[_index] = value;

            ++_index;
        }

        public SliderLog Trim()
        {
            System.Array.Resize(ref this.t, _index);
            System.Array.Resize(ref this.tunity, _index);
            System.Array.Resize(ref this.tdsp, _index);
            System.Array.Resize(ref this.position, _index);
            System.Array.Resize(ref this.value, _index);

            return this;
        }

    }

}