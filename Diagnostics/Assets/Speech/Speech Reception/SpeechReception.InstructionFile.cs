using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

namespace SpeechReception
{
    [JsonObject(MemberSerialization.OptOut)]
    public class InstructionFile
    {
        public string Name { get; set; }
        private bool ShouldSerializeName() { return false; }

        public int Before { get; set; }
        private bool ShouldSerializeBefore() { return false; }
    }

}