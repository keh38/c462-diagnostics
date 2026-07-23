using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace KLib.Logging
{
    [JsonObject(MemberSerialization.OptOut)]
    public class AudioDspLog
    {
        public string name;
        public string T0;
        public double[] blockWallTime;
        public double[] blockDspTime;

        private long _T0;

        public List<AudioDspEventLog> dspEvents = new List<AudioDspEventLog>();

        private readonly int _lengthIncrement = 30000; // ~50 buffer/s => 600 s = 10 minutes
        private int _cursor;

        [JsonIgnore]
        public int CurrentBlockNumber => _cursor;

        public AudioDspLog()
        {
            name = "audioDspLog";
            _T0 = HighPrecisionClock.UtcNowIn100nsTicks;
            T0 = _T0.ToString();
            blockWallTime = new double[_lengthIncrement];
            blockDspTime = new double[_lengthIncrement];
            _cursor = 0;
        }

        public AudioDspLog(string name)
        {
            this.name = name;
            blockWallTime = new double[_lengthIncrement];
            blockDspTime = new double[_lengthIncrement];
            _cursor = 0;
        }

        public void AddBlock()
        {
            if (_cursor >= _lengthIncrement)
            {
                Array.Resize(ref this.blockWallTime, this.blockWallTime.Length + _lengthIncrement);
                Array.Resize(ref this.blockDspTime, this.blockDspTime.Length + _lengthIncrement);
            }
            this.blockWallTime[_cursor] = (double)(HighPrecisionClock.UtcNowIn100nsTicks - _T0);
            this.blockDspTime[_cursor] = AudioSettings.dspTime;
            _cursor++;
        }

        public AudioDspLog Trim()
        {
            Array.Resize(ref this.blockWallTime, _cursor);
            Array.Resize(ref this.blockDspTime, _cursor);
            return this;
        }

        public string ToJsonString()
        {
            Trim();

            string json = $"{{\"{name}\": {{\n";
            json += $"\"T0\": \"{T0}\",\n";
            json += $"\"blockWallTime\": [{string.Join(", ", blockWallTime)}],\n";
            json += $"\"blockDspTime\": [{string.Join(", ", blockDspTime)}]\n";

            foreach (var eventLog in dspEvents)
            {
                eventLog.Trim();
                json += $",\n\"{eventLog.name}\": {{\n";
                json += $"\"blockNumber\": [{string.Join(", ", eventLog.blockNumber)}],\n";
                json += $"\"offset\": [{string.Join(", ", eventLog.offset)}]\n";
                json += "}";
            }
            json += "}}";

            return json;
        }

    }
}
