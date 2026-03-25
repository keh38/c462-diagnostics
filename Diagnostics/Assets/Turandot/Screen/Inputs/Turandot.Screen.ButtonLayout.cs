using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Turandot.Screen
{
    [Serializable]
    public class ButtonLayout : InputLayout
    {
        public enum ButtonStyle { Rectangle, Circle};

        public string Label { set; get; }
        private bool ShouldSerializeLabel() { return false; }

        public int Width { get; set; }
        private bool ShouldSerializeWidth() { return false; }

        public int Height { get; set; }
        private bool ShouldSerializeHeight() { return false; }
        
        public ButtonStyle Style { get; set; }
        private bool ShouldSerializeStyle() { return false; }

        public int FontSize { get; set; }
        private bool ShouldSerializeFontSize() { return false; }

        public KeyCode KeyCode { get; set; }
        private bool ShouldSerializeKeyCode() { return false; }

        public ButtonLayout()
        {
            Name = "Button";
            Label = "Button";
            Width = 300;
            Height = 200;
            Style = ButtonStyle.Rectangle;
            FontSize = 48;
        }

    }
}
