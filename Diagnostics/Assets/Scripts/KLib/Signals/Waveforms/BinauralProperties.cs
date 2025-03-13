using UnityEngine;
using System.Collections;

using Newtonsoft.Json;
using ProtoBuf;

namespace KLib.Signals
{
    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class BinauralProperties
    {
        [ProtoMember(1, IsRequired = true)]
        public float MBL = 0;
        [ProtoMember(2, IsRequired = true)]
        public float ILD = 0;
        [ProtoMember(3, IsRequired = true)]
        public float ITD = 0;
        [ProtoMember(4, IsRequired = true)]
        public float IPD = 0;
        [ProtoMember(5, IsRequired = true)]
        public float balance = 0;

        public BinauralProperties()
        {
        }
    }

}