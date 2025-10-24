using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using JimmysUnityUtilities;

namespace Protocols
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ProtocolHistory
    {
        public string Title { get; set; }

        public List<ProtocolData> Data { get; set; }

        private DateTime _startTime;
        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        public DateTime LatestTime { get; set; }

        [JsonIgnore]
        public bool Finished
        {
            get { return Data?.Find(x => string.IsNullOrEmpty(x.Date)) == null; }
        }

        [JsonIgnore]
        public int NextTextIndex
        {
            get { return Data.FindIndex(x => string.IsNullOrEmpty(x.Date)); }
        }

        public ProtocolHistory() { }
        public ProtocolHistory(Protocol protocol)
        {
            Title = protocol.Title;
            Data = new List<ProtocolData>();
            foreach (var test in protocol.Tests)
            {
                Data.Add(new ProtocolData(test));
            }
            StartTime = DateTime.Now;
            LatestTime = DateTime.Now;
        }

        public bool Matches(Protocol protocol)
        {
            if (protocol.Tests.Count != Data.Count)
            {
                return false;
            }

            bool matches = true;
            for (int k=0; k<protocol.Tests.Count; k++)
            {
                var testParts = protocol.Tests[k].Settings.Split(new char[] { ':' }, 2);
                var historyParts = Data[k].Settings.Split(new char[] { ':' }, 2);

                if (protocol.Tests[k].Scene != Data[k].Scene || testParts[0] != historyParts[0])
                {
                    matches = false;
                    break;
                }
            }

            return matches;
        }
    }

}