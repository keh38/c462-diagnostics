using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class InputSource
    {
        public string name;
        public string label;

        public InputSource(string name) : this(name, name)
        {
        }

        public InputSource(string name, string label)
        {
            this.name = name;
            this.label = label;
        }
    }
}
