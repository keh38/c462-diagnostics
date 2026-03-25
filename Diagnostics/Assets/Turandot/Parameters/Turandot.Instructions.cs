using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Turandot
{

    [JsonObject(MemberSerialization.OptOut)]
    public class Instructions
    {
        public enum HorizontalTextAlignment { Left, Center, Right }
        public enum VerticalTextAlignment { Top, Middle, Bottom }

        public string Text { get; set; }

        public int FontSize { get; set; }
        private bool ShouldSerializeFontSize() { return false; }

        public HorizontalTextAlignment HorizontalAlignment { get; set; }
        private bool ShouldSerializeHorizontalAlignment() { return false; }

        public VerticalTextAlignment VerticalAlignment { get; set; }
        private bool ShouldSerializeVerticalAlignment() { return false; }

        public int LineSpacing { get; set; }
        private bool ShouldSerializeLineSpacing() { return false; }

        public Instructions()
        {
            FontSize = 60;
            HorizontalAlignment = HorizontalTextAlignment.Left;
            VerticalAlignment = VerticalTextAlignment.Top;
            LineSpacing = 1;
        }
    }
}
