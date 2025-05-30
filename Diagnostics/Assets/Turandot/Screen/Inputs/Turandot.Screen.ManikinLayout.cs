using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Turandot.Screen
{
    [Serializable]
    public class ManikinLayout : InputLayout
    {
        [Category("Appearance")]
        public bool ShowLoudness { get; set; }
        private bool ShouldSerializeShowLoudness() { return false; }

        [Category("Appearance")]
        public bool ShowValence { get; set; }
        private bool ShouldSerializeShowValence() { return false; }

        [Category("Appearance")]
        public bool ShowArousal { get; set; }
        private bool ShouldSerializeShowArousal() { return false; }

        [Category("Appearance")]
        public bool ShowDominance { get; set; }
        private bool ShouldSerializeShowDominance() { return false; }

        public ManikinLayout()
        {
            Name = "Manikins";
            ShowLoudness = true;
            ShowValence = true;
            ShowArousal = true;
            ShowDominance = true;
        }

    }
}
