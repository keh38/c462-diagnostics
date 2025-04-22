using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

using KLib;

namespace Protocols
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ProtocolData
    {
        public string Title { get; set; }
        public string Scene { get; set; }
        public string Settings { get; set; }
        public string Date { get; set; }
        public string DataFile { get; set; }

        public ProtocolData() { }
        public ProtocolData(ProtocolEntry entry)
        {
            Title = entry.Title;
            Scene = entry.Scene;
            Settings = entry.Settings;
        }
    }
}