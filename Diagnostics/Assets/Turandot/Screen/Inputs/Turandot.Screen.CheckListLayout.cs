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
        }

    }
}
