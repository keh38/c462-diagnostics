using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Turandot.Inputs
{
    [JsonObject(MemberSerialization.OptOut)]
    public class Keypad : Input
    {
        public int backgroundColor = 0xAAAAAA;

        public Keypad() : base()
        {
        }

    }
}
