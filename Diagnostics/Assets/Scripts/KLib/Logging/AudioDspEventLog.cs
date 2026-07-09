using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KLib.Logging
{
    public class AudioDspEventLog
    {
        public string name;
        public int[] blockNumber;
        public double[] offset;

        private readonly int _lengthIncrement = 15000; // ~50 buffer/s => 300 s = 5 minutes
        private int _cursor;

        public AudioDspEventLog()
        {
            name = "";
            blockNumber = new int[_lengthIncrement];
            offset = new double[_lengthIncrement];
            _cursor = 0;
        }

        public AudioDspEventLog(string name)
        {
            this.name = name;
            blockNumber = new int[_lengthIncrement];
            offset = new double[_lengthIncrement];
            _cursor = 0;
        }

        public void AddEvent(int blockNumber, double offset)
        {
            if (_cursor >= _lengthIncrement)
            {
                Array.Resize(ref this.blockNumber, this.blockNumber.Length + _lengthIncrement);
                Array.Resize(ref this.offset, this.offset.Length + _lengthIncrement);
            }
            this.blockNumber[_cursor] = blockNumber;
            this.offset[_cursor] = offset;
            _cursor++;
        }

        public AudioDspEventLog Trim()
        {
            Array.Resize(ref this.blockNumber, _cursor);
            Array.Resize(ref this.offset, _cursor);
            return this;
        }
    }
}
