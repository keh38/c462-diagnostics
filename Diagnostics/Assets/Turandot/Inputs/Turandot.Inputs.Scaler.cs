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
    public class Scaler : Input
    {
        public int length = 1600;
        public uint bgColor = 0;
        public bool thumb = false;
        public TickSpex tickSpex = new TickSpex();
        public ScaleReference scaleReference = new ScaleReference();

        public Scaler() : base("Scaler")
        {
            bgColor = 0;
        }
    }
}
