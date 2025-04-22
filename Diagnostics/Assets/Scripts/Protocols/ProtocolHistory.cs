using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Protocols
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ProtocolHistory
    {
        public string Title { get; set; }

        public List<ProtocolData> Data { get; set; }

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
                if (protocol.Tests[k].Scene != Data[k].Scene || protocol.Tests[k].Settings != Data[k].Settings)
                {
                    matches = false;
                    break;
                }
            }

            return matches;
        }
    }

}