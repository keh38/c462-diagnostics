using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Cues
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Image : Cue
    {
        public enum HorizontalAlignment { Left, Center, Right};
        public enum VerticalAlignment { Top, Middle, Bottom};

        public string filename;
        public HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center;
        public VerticalAlignment verticalAlignment = VerticalAlignment.Middle;

        public Image() 
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

            if (!string.IsNullOrEmpty(filename))
            {

                string pattern = @"(\-[a-zA-Z]+)";
                Match m = Regex.Match(filename, pattern);
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
            Match m = Regex.Match(filename, pattern);
            while (m.Success)
            {
                filename = filename.Replace(m.Groups[1].Value, "-" + property + value.ToString());
                m = m.NextMatch();
            }
            return "";
        }

    }
}
