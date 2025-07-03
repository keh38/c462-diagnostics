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
        [Category("Behavior")]
        public bool RandomizeOrder { get; set; }
        private bool ShouldSerializeRandomizeOrder() { return false; }

        [Category("Appearance")]
        [DisplayName("Spacing")]
        [Description("Spacing between sliders (pixels)")]
        public int SliderSpacing { get; set; }
        private bool ShouldSerializeSliderSpacing() { return false; }

        [Category("Appearance")]
        [DisplayName("Height")]
        [Description("Slider height (pixe3ls)")]
        public float SliderHeight { get; set; }
        private bool ShouldSerializeSliderHeight() { return false; }

        [Category("Appearance")]
        [DisplayName("Width")]
        [Description("Slider width as fraction of image width (0-1)")]
        public float SliderWidth { get; set; }
        private bool ShouldSerializeSliderWidth() { return false; }

        [Category("Appearance")]
        [DisplayName("Offset")]
        [Description("From top of slider to bottom of image (pixels)")]
        public int SliderVerticalOffset { get; set; }
        private bool ShouldSerializeSliderVerticalOffset() { return false; }
        
        [Category("Appearance")]
        [DisplayName("Font size")]
        [Description("Size of label font")]
        public float SliderFontSize { get; set; }
        private bool ShouldSerializeSliderFontSize() { return false; }

        [Editor(typeof(ManikinCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("Sliders")]
        public ManikinCollection Manikins { get; set; }

        public ManikinLayout()
        {
            Name = "Manikins";
            Manikins = new ManikinCollection();
            RandomizeOrder = false;
            SliderFontSize = 42;
            SliderHeight = 75;
            SliderWidth = 1;
            SliderVerticalOffset = 0;
            SliderSpacing = 50;
        }

    }
}
