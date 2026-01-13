using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using UnityEngine;

namespace Turandot.Screen
{
    [Serializable]
    public class ChecklistLayout : InputLayout
    {
        [Category("Appearance")]
        public int FontSize { get; set; }
        private bool ShouldSerializeFontSize() { return false; }

        [Category("Appearance")]
        [Description("Spacing between prompt and checklist items in pixels")]
        public int PromptSpacing { get; set; }
        private bool ShouldSerializePromptSpacing() { return false; }

        [Category("Behavior")]
        [Description("If true, does not show Apply button if single selection")]
        [DisplayName("Auto advance")]
        public bool AutoAdvance { get; set; }
        private bool ShouldSerializeAutoAdvance() { return false; }

        [Category("Behavior")]
        [Description("If true, allow no selection")]
        [DisplayName("Allow none")]
        public bool AllowNone { get; set; }
        private bool ShouldSerializeAllowNone() { return false; }

        [Category("Behavior")]
        [Description("If true, allow multiple selections")]
        [DisplayName("Allow multiple")]
        public bool AllowMultiple { get; set; }
        private bool ShouldSerializeAllowMultiple() { return false; }

        [Category("Behavior")]
        [Description("If true, never show the apply button")]
        [DisplayName("Disable apply")]
        public bool DisableApply { get; set; }
        private bool ShouldSerializeDisableApply() { return false; }

        [Category("Button")]
        [DisplayName("Font size")]
        public int ButtonFontSize { get; set; }
        private bool ShouldSerializeButtonFontSize() { return false; }

        [Category("Button")]
        [Description("Button width in pixels")]
        [DisplayName("Width")]
        public int ButtonWidth { get; set; }
        private bool ShouldSerializeButtonWidth() { return false; }

        [Category("Button")]
        [Description("Button height in pixels")]
        [DisplayName("Height")]
        public int ButtonHeight { get; set; }
        private bool ShouldSerializeButtonHeight() { return false; }


        [Category("Design")]
        public string Label { set; get; }
        private bool ShouldSerializeLabel() { return false; }

        [Category("Content")]
        public BindingList<string> Items { set; get; }
        private bool ShouldSerializeItems() { return false; }

        public ChecklistLayout()
        {
            Name = "Checklist";
            Label = "Select one";
            FontSize = 48;
            Items = new BindingList<string>();
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
