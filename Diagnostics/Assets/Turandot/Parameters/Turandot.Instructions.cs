using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Instructions
    {
        public string Text { get; set; }
        public int FontSize { get; set; }

        public Instructions()
        {
            FontSize = 48;
        }
    }
}
