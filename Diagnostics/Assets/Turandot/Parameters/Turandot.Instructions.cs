using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Turandot
{

    [JsonObject(MemberSerialization.OptOut)]
    public class Instructions
    {
        public enum HorizontalTextAlignment { Left, Center, Right }
        public enum VerticalTextAlignment { Top, Middle, Bottom }

        [Browsable(false)]
        public string Text { get; set; }

        [Category("Appearance")]
        public int FontSize { get; set; }
        private bool ShouldSerializeFontSize() { return false; }

        [Category("Alignment")]
        [DisplayName("Horizontal")]
        public HorizontalTextAlignment HorizontalAlignment { get; set; }
        private bool ShouldSerializeHorizontalAlignment() { return false; }

        [Category("Alignment")]
        [DisplayName("Vertical")]
        public VerticalTextAlignment VerticalAlignment { get; set; }
        private bool ShouldSerializeVerticalAlignment() { return false; }

        public Instructions()
        {
            FontSize = 60;
            HorizontalAlignment = HorizontalTextAlignment.Left;
            VerticalAlignment = VerticalTextAlignment.Top;
        }
    }
}
