using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

using Turandot.Cues;
using static Turandot.Instructions;

namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class TextBoxLayout : CueLayout
    {
        [Category("Appearance")]
        [Description("")]
        public int FontSize { get; set; }
        private bool ShouldSerializeFontSize() { return false; }

        // https://stackoverflow.com/questions/376234/best-solution-for-xmlserializer-and-system-drawing-color
        // https://stackoverflow.com/questions/38699300/dispaying-and-editing-a-sub-property-of-a-property-in-propertygrid
        [Category("Appearance")]
        [DisplayName("Color")]
        [XmlIgnore]
        public System.Drawing.Color WindowsColor
        {
            get { return System.Drawing.Color.FromArgb(Color); }
            set { Color = value.ToArgb(); }
        }
        private bool ShouldSerializeWindowsColor() { return false; }

        [Category("Alignment")]
        [DisplayName("Text horizontal")]
        public HorizontalTextAlignment HorizontalAlignment { get; set; }
        private bool ShouldSerializeHorizontalAlignment() { return false; }

        [Category("Alignment")]
        [DisplayName("Box vertical")]
        public VerticalTextAlignment BoxVerticalAlignment { get; set; }
        private bool ShouldSerializeBoxVerticalAlignment() { return false; }

        [Browsable(false)]
        public int Color { set; get; }

        public TextBoxLayout() : base()
        {
            Name = "TextBox";
            FontSize = 72;
            Color = KLib.ColorTranslator.ColorInt(0, 0, 0, 1);
        }

        public TextBoxAction GetDefaultCue()
        {
            return new TextBoxAction()
            {
                BeginVisible = true
            };
        }
    }
}
