using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        [Category("Design")]
        public string Label { set; get; }
        private bool ShouldSerializeLabel() { return false; }

        [Category("Content")]
        public BindingList<string> Items { set; get; }
        private bool ShouldSerializeOptions() { return false; }

        public ChecklistLayout()
        {
            Name = "Checklist";
            Label = "Select one";
            FontSize = 48;
            Items = new BindingList<string>();
            AllowMultiple = false;
            AllowNone = false;
            AutoAdvance = false;
        }

    }
}
