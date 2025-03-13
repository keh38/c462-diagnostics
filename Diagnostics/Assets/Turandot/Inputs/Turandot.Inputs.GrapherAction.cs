using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Inputs
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class GrapherAction : Input
    {
        public bool reset = false;
        public bool markStimuli = false;

        public GrapherAction() : base("Grapher")
        {
        }
    }
}
