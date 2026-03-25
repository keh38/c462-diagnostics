using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using UnityEngine;

namespace Turandot.Screen
{
    [Serializable]
    public class ChecklistLayout : InputLayout
    {
        public int FontSize { get; set; }
        private bool ShouldSerializeFontSize() { return false; }

        public int PromptSpacing { get; set; }
        private bool ShouldSerializePromptSpacing() { return false; }

        public bool AutoAdvance { get; set; }
        private bool ShouldSerializeAutoAdvance() { return false; }

        public bool AllowNone { get; set; }
        private bool ShouldSerializeAllowNone() { return false; }

        public bool AllowMultiple { get; set; }
        private bool ShouldSerializeAllowMultiple() { return false; }

        public bool DisableApply { get; set; }
        private bool ShouldSerializeDisableApply() { return false; }

        public int ButtonFontSize { get; set; }
        private bool ShouldSerializeButtonFontSize() { return false; }

        public int ButtonWidth { get; set; }
        private bool ShouldSerializeButtonWidth() { return false; }

        public int ButtonHeight { get; set; }
        private bool ShouldSerializeButtonHeight() { return false; }


        public string Label { set; get; }
        private bool ShouldSerializeLabel() { return false; }

        public List<string> Items { set; get; }
        private bool ShouldSerializeItems() { return false; }

        public ChecklistLayout()
        {
            Name = "Checklist";
            Label = "Select one";
            FontSize = 48;
            Items = new List<string>();
            AllowMultiple = false;
            AllowNone = false;
            AutoAdvance = false;
            PromptSpacing = 50;

            ButtonFontSize = 0;
            ButtonWidth = 200;
            ButtonHeight = 100;
        }

    }
}
