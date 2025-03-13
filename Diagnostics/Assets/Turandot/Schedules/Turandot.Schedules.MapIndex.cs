using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Schedules
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class MapIndex
    {
        public int x = -1;
        public int y = -1;

        public MapIndex()
        {
        }

        public MapIndex(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}