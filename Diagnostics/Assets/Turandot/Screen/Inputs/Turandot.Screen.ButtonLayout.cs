using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Turandot.Screen
{
    [Serializable]
    public class ButtonLayout : InputLayout
    {
        public enum ButtonStyle { Rectangle, Circle};

        [Category("Design")]
        public string Label { set; get; }
        private bool ShouldSerializeLabel() { return false; }

        [Category("Appearance")]
        public int Width { get; set; }
        private bool ShouldSerializeWidth() { return false; }

        [Category("Appearance")]
        public int Height { get; set; }
        private bool ShouldSerializeHeight() { return false; }
        
        [Category("Appearance")]
        public ButtonStyle Style { get; set; }
        private bool ShouldSerializeStyle() { return false; }

        public ButtonLayout()
        {
            Name = "Button";
            Label = "Button";
            Width = 300;
            Height = 200;
            Style = ButtonStyle.Rectangle;
        }

    }
}
