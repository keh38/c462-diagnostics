using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;
using ProtoBuf;

using Turandot.Cues;
namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class MessageLayout : CueLayout
    {
        [Category("Appearance")]
        [Description("")]
        public int FontSize { get; set; }
        private bool ShouldSerializeFontSize() { return false; }

        [Category("Appearance")]
        public uint Color { get; set; }
        private bool ShouldSerializeColor() { return false; }

        public string DefaultText { get; set; }
        private bool ShouldSerializeDefaultText() { return false; }

        public MessageLayout() : base()
        {
            FontSize = 72;
            Color = 0xFF000000;
            DefaultText = "Message";
        }

        public Message GetDefaultCue()
        {
            return new Message()
            {
                Text = "get bent"
            };
        }
    }
}
