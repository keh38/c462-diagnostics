using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Cues
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Message : Cue
    {
        public enum Icon { None, Right, Wrong, Warning}

        [Category("Appearance")]
        public string Text { get; set; }
        private bool ShouldSerializeText() { return false; }

        public int fontSize = 40;

        public Icon icon = Icon.None;
        public int iconSize = 128;
 
        public Message() : this("")
        {
        }

		public Message(string text)
        {
            this.Text = text;
        }

        [XmlIgnore]
        [ProtoIgnore]
        [JsonIgnore]
        override public string Name
        {
            get { return "Message"; }
        }
    }
}
