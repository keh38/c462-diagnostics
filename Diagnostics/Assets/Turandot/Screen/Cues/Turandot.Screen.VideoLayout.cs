using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

using Turandot.Cues;

namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class VideoLayout : CueLayout
    {
        public float Width { get; set; }
        private bool ShouldSerializeWidth() { return false; }

        public float Height { get; set; }
        private bool ShouldSerializeHeight() { return false; }


        public VideoLayout()
        {
            Width = 0.9f;
            Height = 0.9f;
        }

        public VideoAction GetDefaultCue()
        {
            return new VideoAction()
            {
                BeginVisible = true,
                EndVisible = true
            };
        }
    }
}
