using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Cues
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Help : Cue
    {
        public string text;
        public float x =    0;
        public float y =  100;
        public float w = 1750;
        public float h =  800;

        public Help() : this("")
        {
        }

		public Help(string text)
        {
            this.text = text;
            this.changeAppearance = false;
        }

        [XmlIgnore]
        [ProtoIgnore]
        [JsonIgnore]
        override public string Name
        {
            get { return "Help"; }
        }

    }
}
