using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Cues
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class VideoAction : Cue
    {
        [Category("Content")]
        public string Filename { get; set; }
        private bool ShouldSerializeFilename() { return false; }

        public VideoAction() 
        {
        }

        [XmlIgnore]
        [ProtoIgnore]
        [JsonIgnore]
        override public string Name
        {
            get { return "Image"; }
        }

        [JsonIgnore]
        override public bool IsSequenceable
        {
            get { return BeginVisible || EndVisible; }
        }

        override public List<string> GetPropertyNames()
        {
            var names = new List<string>();

            if (!string.IsNullOrEmpty(Filename))
            {

                string pattern = @"(\-[a-zA-Z]+)";
                Match m = Regex.Match(Filename, pattern);
                while (m.Success)
                {
                    names.Add(Name + "." + m.Groups[1].Value.Substring(1));
                    m = m.NextMatch();
                }
            }
            return names;
        }

        public override string SetProperty(string property, float value)
        {
            string pattern = @"(\-" + property + "[0-9]+)";
            Match m = Regex.Match(Filename, pattern);
            while (m.Success)
            {
                Filename = Filename.Replace(m.Groups[1].Value, "-" + property + value.ToString());
                m = m.NextMatch();
            }
            return "";
        }

    }
}
