using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Pupillometry
{
    [JsonObject(MemberSerialization.OptOut)]
    public class DynamicRangeData
    {
        public double[] time;
        public float[] intensity;

        [JsonIgnore]
        private int _index;
        [JsonIgnore]
        private int _lengthIncrement;

        public DynamicRangeData() { }
        public DynamicRangeData(string filePath, int lengthIncrement)
        {
            _lengthIncrement = lengthIncrement;
            time = new double[_lengthIncrement];
            intensity = new float[_lengthIncrement];
            _index = 0;
        }

        public void Add(double t, float intensity)
        {
            if (_index == this.time.Length)
            {
                int newLen = this.time.Length + _lengthIncrement;
                System.Array.Resize(ref this.time, newLen);
                System.Array.Resize(ref this.intensity, newLen);
            }
            this.time[_index] = t;
            this.intensity[_index] = intensity;
            _index++;
        }

        public void Trim()
        {
            System.Array.Resize(ref this.time, _index);
            System.Array.Resize(ref this.intensity, _index);
        }
    }
}