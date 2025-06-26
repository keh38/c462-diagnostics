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
        [Category("Behavior")]
        [Description("If true, does not show Apply button if single selection")]
        [DisplayName("Auto advance")]
        public bool AutoAdvance { get; set; }

        [Category("Behavior")]
        [Description("If true, allow multiple selections")]
        [DisplayName("Allow multiple")]
        public bool AllowMultiple { get; set; }
        private bool ShouldSerializeAllowMultiple() { return false; }

        private bool ShouldSerializeAutoAdvance() { return false; }
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
            Items = new BindingList<string>();
        }

    }
}
