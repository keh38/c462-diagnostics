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

    }

}