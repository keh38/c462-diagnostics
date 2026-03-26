using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Turandot.Inputs
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ScaleReference
    {
        public enum RefType { None, Tick };

        public RefType refType = RefType.None;
        public string location = "0.5";
        public bool startAt = false;
        public uint color = 0xFF000000;
        public string label = "";

        public ScaleReference()
        {
        }
    }
}
