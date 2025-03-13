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
    public class RandomProcess : Input
    {
        public string intervalExpr = "";
        public string valueExpr = "";

        public RandomProcess() : base("Random process")
        {
        }

    }
}
