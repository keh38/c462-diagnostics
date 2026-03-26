using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Turandot.Schedules
{
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