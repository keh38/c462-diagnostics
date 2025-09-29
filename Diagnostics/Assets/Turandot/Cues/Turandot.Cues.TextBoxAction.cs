using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Cues
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class TextBoxAction : Cue
    {
        [Category("Appearance")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Markdown { get; set; }
        private bool ShouldSerializeMarkdown() { return false; }

        public TextBoxAction()
        {
            Markdown = string.Empty;
        }

        [XmlIgnore]
        [ProtoIgnore]
        [JsonIgnore]
        override public string Name
        {
            get { return "TextBox"; }
        }
    }
}
